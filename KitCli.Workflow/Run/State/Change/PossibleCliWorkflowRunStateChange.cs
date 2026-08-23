using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Run.State.Change;

/// <summary>
/// One entry in the fixed table of legal state transitions: a status a run may start at, and a
/// status it is allowed to move to from there.
/// </summary>
/// <param name="ifStartedAt">The status a run must currently be at for this entry to apply.</param>
/// <param name="canMoveTo">The status a run at <paramref name="ifStartedAt"/> is allowed to transition to.</param>
public class PossibleCliWorkflowRunStateChange(ClIWorkflowRunStateStatus ifStartedAt, ClIWorkflowRunStateStatus canMoveTo)
{
    /// <summary>The status a run must currently be at for this entry to apply.</summary>
    public readonly ClIWorkflowRunStateStatus IfStartedAt = ifStartedAt;

    /// <summary>The status a run at <see cref="IfStartedAt"/> is allowed to transition to.</summary>
    public readonly ClIWorkflowRunStateStatus CanMoveTo = canMoveTo;
}