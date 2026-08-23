using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

/// <summary>
/// A state change recorded when a run transitions as a result of a command execution completing,
/// carrying the outcomes that drove the transition.
/// </summary>
public class OutcomeCliWorkflowRunStateChange : CliWorkflowRunStateChange, IOutcomeCliWorkflowRunStateChange
{
    /// <inheritdoc/>
    public Outcome[] Outcomes { get; }

    /// <summary>Creates a state change record for a transition triggered by command outcomes.</summary>
    /// <param name="at">Elapsed time, from the run's stopwatch, at which the transition occurred.</param>
    /// <param name="from">The status transitioned from.</param>
    /// <param name="to">The status transitioned to.</param>
    /// <param name="outcomes">The outcomes that triggered this transition.</param>
    public OutcomeCliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to,
        Outcome[] outcomes) : base(at, from, to)
    {
        Outcomes = outcomes;
    }
}