using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

public abstract class ArgsCliApp : CliApp
{
    protected ArgsCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

    public async Task Run(List<IOutcomeIoWriter> outcomeIoWriters, string[] args)
    {
        OnSessionStart();
        
        Io.Pause();
        
        SetUpEventHandlers();
        
        var run = Workflow.NextRun();
            
        OnRunCreated(run);
            
        var ask = string.Join(" ",  args);
        
        var runTask =  run.RespondToAsk(ask);

        OnRunStarted(run, ask);
        
        var outcomes = await runTask;
            
        WriteOutcomes(outcomes, outcomeIoWriters);
            
        OnRunComplete(run, outcomes);
            
        Workflow.Stop();
        
        OnSessionEnd(Workflow.Runs);
    }
}