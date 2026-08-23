using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Run.State.Change;

/// <summary>
/// Default <see cref="ICliWorkflowRunStateChange"/> implementation: a plain from/to/timestamp
/// record of one transition, with no additional payload attached.
/// </summary>
public class CliWorkflowRunStateChange : ICliWorkflowRunStateChange
{
    /// <inheritdoc/>
    public TimeSpan At { get; }

    /// <inheritdoc/>
    public ClIWorkflowRunStateStatus From { get; }

    /// <inheritdoc/>
    public ClIWorkflowRunStateStatus To { get; }

    /// <summary>Creates a state change record for a transition with no attached payload.</summary>
    /// <param name="at">Elapsed time, from the run's stopwatch, at which the transition occurred.</param>
    /// <param name="from">The status transitioned from.</param>
    /// <param name="to">The status transitioned to.</param>
    public CliWorkflowRunStateChange(
        TimeSpan at,
        ClIWorkflowRunStateStatus from,
        ClIWorkflowRunStateStatus to)
    {
        At = at;
        From = from;
        To = to;
    }
}