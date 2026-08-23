using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Abstractions.Run.State.Change;

/// <summary>
/// A recorded state transition that was triggered by a command execution completing, carrying
/// the outcomes that caused it.
/// </summary>
public interface IOutcomeCliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    /// <summary>The outcomes that triggered this transition.</summary>
    Outcome[] Outcomes { get; }
}