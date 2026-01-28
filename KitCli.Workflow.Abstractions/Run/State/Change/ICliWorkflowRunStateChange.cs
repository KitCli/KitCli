namespace KitCli.Workflow.Abstractions.Run.State.Change;

public interface ICliWorkflowRunStateChange
{
    TimeSpan At { get; }
    ClIWorkflowRunStateStatus From { get; }
    ClIWorkflowRunStateStatus To { get; }
}