using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

public class CliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    public TimeSpan At { get; }
    public ClIWorkflowRunStateStatus From { get; }
    public ClIWorkflowRunStateStatus To { get; }
    
    public CliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to)
    {
        At = at;
        From = from;
        To = to;
    }
}