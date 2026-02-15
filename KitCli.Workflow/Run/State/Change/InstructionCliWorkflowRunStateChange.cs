using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

public class InstructionCliWorkflowRunStateChange : CliWorkflowRunStateChange, IInstructionCliWorkflowRunStateChange
{
    public Instruction Instruction { get;  }

    public InstructionCliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to,
        Instruction instruction)
        : base(at, from, to)
    {
        Instruction = instruction;
    }
}