namespace KitCli.Workflow.Abstractions;

public interface ICliWorkflow
{
    CliWorkflowStatus Status { get; }
    
    List<ICliWorkflowRun> Runs { get; }
    
    ICliWorkflowRun NextRun();

    void Stop();
}