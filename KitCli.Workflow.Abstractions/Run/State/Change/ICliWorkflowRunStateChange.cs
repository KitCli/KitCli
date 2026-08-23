namespace KitCli.Workflow.Abstractions.Run.State.Change;

/// <summary>
/// A single recorded transition in a workflow run's state history: which status it moved from,
/// which it moved to, and when.
/// </summary>
public interface ICliWorkflowRunStateChange
{
    /// <summary>Elapsed time, from the run's stopwatch, at which this transition occurred.</summary>
    TimeSpan At { get; }

    /// <summary>The status the run transitioned from.</summary>
    ClIWorkflowRunStateStatus From { get; }

    /// <summary>The status the run transitioned to.</summary>
    ClIWorkflowRunStateStatus To { get; }
}