using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Abstractions;

public interface ICliWorkflowRun
{
    ICliWorkflowRunState State { get; }
    ValueTask<Outcome[]> RespondToAsk(string? ask);
    ValueTask<Outcome[]> RespondToNext();
}