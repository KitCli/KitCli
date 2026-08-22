using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public abstract class TerminalCliApp : CliApp
{
    protected TerminalCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

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