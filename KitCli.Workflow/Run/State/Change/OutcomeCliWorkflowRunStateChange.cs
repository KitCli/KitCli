using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

public class OutcomeCliWorkflowRunStateChange : CliWorkflowRunStateChange, IOutcomeCliWorkflowRunStateChange
{
    public Outcome[] Outcomes { get; }
    
    public OutcomeCliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to,
        Outcome[] outcomes) : base(at, from, to)
    {
        Outcomes = outcomes;
    }
}