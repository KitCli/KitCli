using System.Runtime.ExceptionServices;
using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// A KitCli app: an interactive session that asks the user for something, runs it to the end, and asks
/// again, until a command stops the workflow or the input ends. Running an ask to the end means every
/// step the run queues behind it too — a chained command, another page of a table.
/// <see cref="HeadlessCliApp"/> overrides <see cref="Run"/> to run a single ask with nobody to prompt.
/// A set of <c>protected virtual</c> hooks lets a consuming application observe a run without
/// reimplementing any of this.
/// </summary>
public abstract class CliApp
{
    /// <summary>The workflow that creates and tracks this app's runs, and owns the shared cancellation token.</summary>
    protected readonly ICliWorkflow Workflow;

    /// <summary>The I/O seam used to source asks, write output, and register cancellation, without touching <see cref="Console"/> directly.</summary>
    protected readonly ICliIo Io;

    /// <summary>
    /// Initializes the shared workflow and I/O references used by subclasses.
    /// </summary>
    /// <param name="workflow">The workflow driving this app's runs.</param>
    /// <param name="io">The I/O implementation used to ask, write, and observe cancellation.</param>
    protected CliApp(ICliWorkflow workflow, ICliIo io)
    {
        Workflow = workflow;
        Io = io;
    }

    /// <summary>
    /// Runs the session: asks the user for something, executes it, and asks again, until a command
    /// stops the workflow or <see cref="ICliIo.AskAsync"/> reports the end of the input.
    /// </summary>
    /// <param name="outcomeIoWriters">The writers used to render each run's outcomes.</param>
    /// <param name="args">The process args, for an app whose ask comes from them; unused here.</param>
    /// <returns>A task that completes once the session has ended.</returns>
    public virtual async Task Run(List<IOutcomeIoWriter> outcomeIoWriters, string[]? args = null)
    {
        OnSessionStart();

        Io.Pause();

        SetUpEventHandlers();

        while (Workflow.Status != CliWorkflowStatus.Stopped)
        {
            var ask = await Io.AskAsync(Workflow.CancellationToken);

            if (ask is null)
            {
                break;
            }

            await ExecuteRunOperation(ask, outcomeIoWriters);
        }

        OnSessionEnd(Workflow.Runs);
    }

    /// <summary>
    /// Responds to one ask via <see cref="ICliWorkflowRun.RespondToAsk"/>, then keeps moving the run
    /// past each step it queues via <see cref="ICliWorkflowRun.MoveToNext"/> — a chained command,
    /// another page of a table — writing the outcomes of each. Returns once the run has nothing
    /// queued, which is what makes a chain arrive whole whether or not anyone can be asked again.
    /// </summary>
    /// <param name="ask">The ask to respond to.</param>
    /// <param name="outcomeIoWriters">The writers used to render the outcomes.</param>
    /// <returns>The outcomes of the last step the run took.</returns>
    protected async Task<Outcome[]> ExecuteRunOperation(string ask, List<IOutcomeIoWriter> outcomeIoWriters)
    {
        var run = Workflow.NextRun();

        OnRunCreated(run);

        var runTask = run.RespondToAsk(ask);

        OnRunStarted(run, ask);

        var outcomes = RethrowIfExceptional(await runTask);

        WriteOutcomes(outcomes, outcomeIoWriters);

        OnRunComplete(run, outcomes);

        Io.Pause();

        while (run.State.Changes[^1].To == ClIWorkflowRunStateStatus.MovePastAsk)
        {
            var movePastAskTask = run.MoveToNext();

            OnMovingPastAsk(run);

            outcomes = RethrowIfExceptional(await movePastAskTask);

            WriteOutcomes(outcomes, outcomeIoWriters);

            OnRunComplete(run, outcomes);

            Io.Pause();
        }

        return outcomes;
    }

    /// <summary>
    /// Rethrows the original exception behind an <see cref="ExceptionOutcome"/> so an unexpected
    /// command failure ends the whole session, instead of silently continuing to the next ask or
    /// exiting as though the work succeeded. Unlike an invalid ask, an <c>Exceptional</c> run means
    /// something the app didn't account for happened, and that shouldn't be masked.
    /// </summary>
    private static Outcome[] RethrowIfExceptional(Outcome[] outcomes)
    {
        var exceptionOutcome = outcomes.OfType<ExceptionOutcome>().SingleOrDefault();

        if (exceptionOutcome is not null)
        {
            ExceptionDispatchInfo.Capture(exceptionOutcome.Exception).Throw();
        }

        return outcomes;
    }

    /// <summary>
    /// Wires <see cref="ICliIo.OnCancel"/> so a cancellation request (e.g. Ctrl+C) calls
    /// <see cref="ICliWorkflow.InterruptCurrentRun"/>. Called once, at the top of <see cref="Run"/>.
    /// </summary>
    protected void SetUpEventHandlers()
    {
        Io.OnCancel(Workflow.InterruptCurrentRun);
    }

    /// <summary>
    /// Writes each outcome using the first <paramref name="outcomeIoWriters"/> entry whose
    /// <see cref="IOutcomeIoWriter.CanWriteFor"/> returns <c>true</c> for it. An outcome with no
    /// matching writer is silently skipped.
    /// </summary>
    /// <param name="outcomes">The outcomes produced by a completed run.</param>
    /// <param name="outcomeIoWriters">The writers to match against, tried in list order.</param>
    private void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
    {
        foreach (var outcome in outcomes)
        {
            var writer = outcomeIoWriters
                .FirstOrDefault(w => w.CanWriteFor(outcome));

            writer?.Write(outcome);
        }
    }

    /// <summary>
    /// Called once, before the host loop starts. No-op by default; override for side effects such as
    /// logging or telemetry.
    /// </summary>
    protected virtual void OnSessionStart()
    {
    }

    /// <summary>
    /// Called after <see cref="ICliWorkflow.NextRun"/> hands back a run, before that run is driven.
    /// No-op by default; override for side effects such as logging or telemetry.
    /// </summary>
    /// <param name="run">The run that was just created or resumed.</param>
    protected virtual void OnRunCreated(ICliWorkflowRun run)
    {
    }

    /// <summary>
    /// Called after a run's ask-handling task has been started but before it is awaited, so an
    /// override can show a "working…" indicator concurrently with the run executing. No-op by default.
    /// </summary>
    /// <param name="run">The run that was started.</param>
    /// <param name="ask">The ask text the run is responding to, or <c>null</c> if none was sourced.</param>
    protected virtual void OnRunStarted(ICliWorkflowRun run, string? ask)
    {
    }

    /// <summary>
    /// Called when a run continues past a queued <c>MovePastAsk</c> state change instead of sourcing a
    /// fresh ask — paging in an interactive session, a chained step in either. No-op by default.
    /// </summary>
    /// <param name="run">The run that is moving past its ask.</param>
    protected virtual void OnMovingPastAsk(ICliWorkflowRun run)
    {
    }

    /// <summary>
    /// Called after a run's outcomes have been written, once the run has finished. No-op by default;
    /// override for side effects such as logging or telemetry.
    /// </summary>
    /// <param name="run">The run that completed.</param>
    /// <param name="outcomes">The outcomes the run produced.</param>
    protected virtual void OnRunComplete(ICliWorkflowRun run, Outcome[] outcomes)
    {
    }

    /// <summary>
    /// Called once, after the host loop has stopped. No-op by default; override for side effects such
    /// as logging or telemetry.
    /// </summary>
    /// <param name="runs">All runs the workflow created during the session.</param>
    protected virtual void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
    }
}
