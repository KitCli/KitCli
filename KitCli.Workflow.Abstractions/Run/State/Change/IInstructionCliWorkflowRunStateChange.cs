using KitCli.Instructions.Abstractions;

namespace KitCli.Workflow.Abstractions.Run.State.Change;

/// <summary>
/// A recorded state transition that was triggered by parsing an instruction, carrying the
/// instruction that caused it.
/// </summary>
public interface IInstructionCliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    /// <summary>The instruction that triggered this transition.</summary>
    Instruction Instruction { get; }
}