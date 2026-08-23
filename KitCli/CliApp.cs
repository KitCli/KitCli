using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// Shared shell for a KitCli host loop. Owns the <see cref="ICliWorkflow"/>/<see cref="ICliIo"/>
/// references, cancellation wiring, and outcome writing that both <see cref="TerminalCliApp"/> and
/// <see cref="ArgsCliApp"/> need, plus a set of lifecycle hooks a consuming application can override
/// to observe the loop without reimplementing it. <see cref="CliApp"/> itself defines no <c>Run</c>
/// method or loop — each subclass supplies that.
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
    /// Wires <see cref="ICliIo.OnCancel"/> so a cancellation request (e.g. Ctrl+C) calls
    /// <see cref="ICliWorkflow.InterruptCurrentRun"/>. Called once, at the top of a subclass's <c>Run</c>.
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
    protected void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
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
    /// fresh ask (terminal-mode paging/continuation). No-op by default.
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