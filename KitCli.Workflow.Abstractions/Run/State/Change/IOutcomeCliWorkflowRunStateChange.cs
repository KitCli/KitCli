using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Abstractions.Run.State.Change;

public interface IOutcomeCliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    CliCommandOutcome[] Outcomes { get; }
}