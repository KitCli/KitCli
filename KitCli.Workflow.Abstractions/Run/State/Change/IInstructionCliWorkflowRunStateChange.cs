using KitCli.Instructions.Abstractions;

namespace KitCli.Workflow.Abstractions.Run.State.Change;

public interface IInstructionCliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    CliInstruction Instruction { get; }
}