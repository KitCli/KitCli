using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

public class OutcomeCliWorkflowRunStateChange : CliWorkflowRunStateChange, IOutcomeCliWorkflowRunStateChange
{
    public CliCommandOutcome[] Outcomes { get; }
    
    public OutcomeCliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to,
        CliCommandOutcome[] outcomes) : base(at, from, to)
    {
        Outcomes = outcomes;
    }
}