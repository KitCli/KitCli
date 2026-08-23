namespace KitCli.Workflow.Abstractions;

/// <summary>
/// The statuses a <see cref="ICliWorkflowRun"/> can occupy, and which the run's state machine
/// enforces legal transitions between.
/// </summary>
public enum ClIWorkflowRunStateStatus
{
    /// <summary>The run has been constructed but has not yet processed an ask.</summary>
    Created,

    /// <summary>The run has a valid instruction/command and is executing it.</summary>
    Running,

    /// <summary>The ask was empty/null, failed validation, or resolved to no known command.</summary>
    InvalidAsk,

    /// <summary>The command handler, or the machinery around it, threw an exception.</summary>
    Exceptional,

    /// <summary>
    /// The last outcome returned was a <c>NextCliCommandOutcome</c>; the run is waiting on a
    /// <see cref="ICliWorkflowRun.MoveToNext"/> call rather than a fresh ask.
    /// </summary>
    MovePastAsk,

    /// <summary>A <see cref="ICliWorkflowRun.MoveToNext"/> call was made when there was no valid outcome to move past.</summary>
    InvalidMovePastAsk,

    /// <summary>
    /// The last outcome returned was reusable (but not a <c>NextCliCommandOutcome</c>); the run
    /// keeps its accumulated context available for the next ask.
    /// </summary>
    ReachedReusableOutcome,

    /// <summary>The run reached an outcome that is not reusable, ending its arc of commands.</summary>
    ReachedFinalOutcome,

    /// <summary>
    /// The run is over: one of <see cref="ReachedFinalOutcome"/>, <see cref="InvalidAsk"/>, or
    /// <see cref="Exceptional"/> was reached, and the run's DI scope has been disposed.
    /// </summary>
    Finished,
}