using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public abstract class CliApp
{
    private readonly ICliWorkflow _workflow;
    protected readonly ICliIo Io;

    protected CliApp(ICliWorkflow workflow, ICliIo io)
    {
        _workflow = workflow;
        Io = io;
    }

    public async Task Run(List<IOutcomeIoWriter> outcomeIoWriters)
    { 
        OnSessionStart();
        
        Io.Pause();
        
        SetUpEventHandlers();
        
        while (_workflow.Status != CliWorkflowStatus.Stopped)
        {
            var run = _workflow.NextRun();
            
            OnRunCreated(run);

            var outcomes = await ExecuteRunOperation(run);

            WriteOutcomes(outcomes, outcomeIoWriters);
            
            OnRunComplete(run, outcomes);
            
            Io.Pause();
        }
        
        OnSessionEnd(_workflow.Runs);
    }
    
    private void SetUpEventHandlers()
    {
        Io.OnCancel(() =>
        {
            _workflow.Stop();

            OnSessionEnd(_workflow.Runs);

            Environment.Exit(exitCode: 0);
        });
    }

    private ValueTask<Outcome[]> ExecuteRunOperation(ICliWorkflowRun run)
    {
        var shouldMovePastAsk = run
            .State
            .WasChangedTo(ClIWorkflowRunStateStatus.MovePastAsk);

        if (shouldMovePastAsk)
        {
            var movePastAskTask = run.MoveToNext();

            OnMovingPastAsk(run);

            return movePastAskTask;
        }

        var ask = Io.Ask();

        var runTask =  run.RespondToAsk(ask);

        OnRunStarted(run, ask);

        return runTask;
    }

    private void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
    {
        foreach (var outcome in outcomes)
        {
            var writer = outcomeIoWriters
                .FirstOrDefault(w => w.CanWriteFor(outcome));

            writer?.Write(outcome);
        }
    }

    protected virtual void OnSessionStart()
    {
    }

    protected virtual void OnRunCreated(ICliWorkflowRun run)
    {
    }

    protected virtual void OnRunStarted(ICliWorkflowRun run, string? ask)
    {
    }

    protected virtual void OnMovingPastAsk(ICliWorkflowRun run)
    {
    }

    protected virtual void OnRunComplete(ICliWorkflowRun run, Outcome[] outcomes)
    {
    }

    protected virtual void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
    }
}