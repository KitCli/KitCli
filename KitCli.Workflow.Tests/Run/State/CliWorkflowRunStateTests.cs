using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Run.State;

namespace KitCli.Workflow.Tests.Run.State;

public abstract class CliWorkflowRunStateTests
{
    protected static CliWorkflowRunState GetPreparedState(IEnumerable<ClIWorkflowRunStateStatus> priorStatuses)
    {
        var state = new CliWorkflowRunState();
        
        foreach (var priorStatus in priorStatuses)
        {
            ChangeStateStatus(state, priorStatus);
        }

        return state;
    }

    private static void ChangeStateStatus(CliWorkflowRunState state, ClIWorkflowRunStateStatus status)
    {
        if (status is ClIWorkflowRunStateStatus.ReachedReusableOutcome)
        {
            var reusableOutcome = new SayOutcome(
                nameof(ClIWorkflowRunStateStatus.ReachedReusableOutcome));
                
            state.ChangeTo(status, [reusableOutcome]);
            return;
        }

        state.ChangeTo(status);
    }
}