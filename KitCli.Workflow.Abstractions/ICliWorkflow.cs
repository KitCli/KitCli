namespace KitCli.Workflow.Abstractions;

public interface ICliWorkflow
{
    CliWorkflowStatus Status { get; }

    CancellationToken CancellationToken { get; }

    List<ICliWorkflowRun> Runs { get; }

    ICliWorkflowRun NextRun();

    void Stop();

    void InterruptCurrentRun();
}