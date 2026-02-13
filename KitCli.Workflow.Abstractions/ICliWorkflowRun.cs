using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Abstractions;

public interface ICliWorkflowRun
{
    ICliWorkflowRunState State { get; }
    ValueTask<CliCommandOutcome[]> RespondToAsk(string? ask);
    ValueTask<CliCommandOutcome[]> RespondToNext();
}