using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Tests.TestHelpers;

/// <summary>
/// A run that only holds a history, for tests that read runs rather than drive them. Asking it to do
/// anything throws.
/// </summary>
public class TestCliWorkflowRun(ICliWorkflowRunState state) : ICliWorkflowRun
{
    public ICliWorkflowRunState State { get; } = state;

    public ValueTask<Outcome[]> RespondToAsk(string? ask)
        => throw new NotSupportedException();

    public ValueTask<Outcome[]> MoveToNext()
        => throw new NotSupportedException();
}
