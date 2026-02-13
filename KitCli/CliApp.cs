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

    public async Task Run(List<ICliCommandOutcomeIoWriter> outcomeIoWriters)
    { 
        OnSessionStart();
        
        Io.Pause();
        
        Io.OnCancel(() =>
        {
            _workflow.Stop();
            
            OnSessionEnd(_workflow.Runs);
            
            Environment.Exit(exitCode: 0);
        });
        
        while (_workflow.Status != CliWorkflowStatus.Stopped)
        {
            var run = _workflow.NextRun();
            
            OnRunCreated(run);

            var movePastAsk = run.State.WasChangedTo(ClIWorkflowRunStateStatus.MovePastAsk);

            var ask = !movePastAsk
                ? Io.Ask()
                : null;
            
            var runTask =  !movePastAsk 
                ? run.RespondToAsk(ask)
                : run.RespondToNext();
            
            OnRunStarted(run, ask);

            var outcomes = await runTask;

            WriteOutcomes(outcomes, outcomeIoWriters);
            
            OnRunComplete(run, outcomes);
            
            Io.Pause();
        }
        
        OnSessionEnd(_workflow.Runs);
    }
    
    private void WriteOutcomes(CliCommandOutcome[] outcomes, List<ICliCommandOutcomeIoWriter> outcomeIoWriters)
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

    protected virtual void OnRunComplete(ICliWorkflowRun run, CliCommandOutcome[] outcomes)
    {
    }
    
    protected virtual void OnSessionEnd(List<ICliWorkflowRun> runs)
    {
    }
}