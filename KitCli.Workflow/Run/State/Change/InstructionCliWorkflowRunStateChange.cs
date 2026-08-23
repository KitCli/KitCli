using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

/// <summary>
/// A state change recorded when a run transitions as a result of parsing/resolving an
/// instruction, carrying that instruction alongside the transition.
/// </summary>
public class InstructionCliWorkflowRunStateChange : CliWorkflowRunStateChange, IInstructionCliWorkflowRunStateChange
{
    /// <inheritdoc/>
    public Instruction Instruction { get;  }

    /// <summary>Creates a state change record for a transition triggered by an instruction.</summary>
    /// <param name="at">Elapsed time, from the run's stopwatch, at which the transition occurred.</param>
    /// <param name="from">The status transitioned from.</param>
    /// <param name="to">The status transitioned to.</param>
    /// <param name="instruction">The instruction that triggered this transition.</param>
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