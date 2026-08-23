using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// Entry point for an interactive KitCli session. Repeatedly sources an ask (or continues a run that
/// queued a <c>MovePastAsk</c> continuation), drives it to completion, writes its outcomes, and pauses
/// between iterations, until the workflow's <see cref="CliWorkflowStatus"/> becomes
/// <see cref="CliWorkflowStatus.Stopped"/>.
/// </summary>
public abstract class TerminalCliApp : CliApp
{
    /// <summary>
    /// Initializes the shared workflow and I/O references used by the interactive loop.
    /// </summary>
    /// <param name="workflow">The workflow driving this app's runs.</param>
    /// <param name="io">The I/O implementation used to ask, write, and observe cancellation.</param>
    protected TerminalCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

    /// <summary>
    /// Runs the interactive host loop: fires <see cref="CliApp.OnSessionStart"/>, wires cancellation,
    /// then repeatedly asks for (or resumes) a run, writes its outcomes, and pauses, until the
    /// workflow stops. Fires <see cref="CliApp.OnSessionEnd"/> once the loop exits.
    /// </summary>
    /// <param name="outcomeIoWriters">The writers used to render each run's outcomes.</param>
    /// <param name="args">Unused by the interactive loop; accepted so callers can pass through process args uniformly.</param>
    /// <returns>A task that completes once the session has ended.</returns>
    public async Task Run(List<IOutcomeIoWriter> outcomeIoWriters, string[]? args = null)
    {
        OnSessionStart();
        
        Io.Pause();
        
        SetUpEventHandlers();

        while (Workflow.Status != CliWorkflowStatus.Stopped)
        {
            var run = Workflow.NextRun();

            OnRunCreated(run);

            var outcomes = await ExecuteRunOperation(run);

            WriteOutcomes(outcomes, outcomeIoWriters);

            OnRunComplete(run, outcomes);

            Io.Pause();
        }

        OnSessionEnd(Workflow.Runs);
    }

    /// <summary>
    /// Drives one run: continues past a queued <c>MovePastAsk</c> state change via
    /// <see cref="ICliWorkflowRun.MoveToNext"/> if one is recorded, otherwise sources a fresh ask via
    /// <see cref="ICliIo.AskAsync"/> and responds to it via <see cref="ICliWorkflowRun.RespondToAsk"/>.
    /// </summary>
    /// <param name="run">The run to drive.</param>
    /// <returns>The outcomes produced by the run.</returns>
    private async ValueTask<Outcome[]> ExecuteRunOperation(ICliWorkflowRun run)
    {
        var shouldMovePastAsk = run
            .State
            .WasChangedTo(ClIWorkflowRunStateStatus.MovePastAsk);

        if (shouldMovePastAsk)
        {
            var movePastAskTask = run.MoveToNext();

            OnMovingPastAsk(run);

            return await movePastAskTask;
        }

        var ask = await Io.AskAsync(Workflow.CancellationToken);

        var runTask = run.RespondToAsk(ask);

        OnRunStarted(run, ask);

        return await runTask;
    }
}