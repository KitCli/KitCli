using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Run.State.Change;

public class PossibleCliWorkflowRunStateChange(ClIWorkflowRunStateStatus ifStartedAt, ClIWorkflowRunStateStatus canMoveTo)
{
    public readonly ClIWorkflowRunStateStatus IfStartedAt = ifStartedAt;
    public readonly ClIWorkflowRunStateStatus CanMoveTo = canMoveTo;
}