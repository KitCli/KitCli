using KitCli.Commands.Abstractions.Io.Outcomes;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public abstract class CliApp
{
    private readonly ICliWorkflow _workflow;
    protected readonly ICliCommandOutcomeIo Io;

    protected CliApp(ICliWorkflow workflow, ICliCommandOutcomeIo io)
    {
        _workflow = workflow;
        Io = io;
    }
    
    public async Task Run()
    { 
        OnSessionStart();
        
        while (_workflow.Status != CliWorkflowStatus.Stopped)
        {
            var run = _workflow.NextRun();
            
            OnRunCreated(run);
            
            var ask = Io.Ask();
            
            var runTask =  run.RespondToAsk(ask);
            
            OnRunStarted(run, ask);

            var outcomes = await runTask;
            
            Io.Say(outcomes);
            
            OnRunComplete(run, outcomes);
        }
        
        OnSessionEnd(_workflow.Runs);
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