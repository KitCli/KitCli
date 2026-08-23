namespace KitCli.Workflow.Abstractions;

/// <summary>
/// Top-level object driving a CLI's asks and command executions. Holds the list of runs that
/// have occurred so far and hands the host the run it should currently be interacting with.
/// </summary>
public interface ICliWorkflow
{
    /// <summary>Whether the workflow is still accepting asks or has been stopped.</summary>
    CliWorkflowStatus Status { get; }

    /// <summary>
    /// The cancellation token shared by every run this workflow creates; cancelling it signals
    /// cooperative cancellation to whichever run is currently executing.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>Every run this workflow has created so far, in creation order.</summary>
    List<ICliWorkflowRun> Runs { get; }

    /// <summary>
    /// Returns the run the caller should interact with next: the single existing run that hasn't
    /// yet reached <see cref="ClIWorkflowRunStateStatus.ReachedFinalOutcome"/>, if one exists,
    /// otherwise a newly created run.
    /// </summary>
    /// <returns>The run to call <c>RespondToAsk</c> or <c>MoveToNext</c> on next.</returns>
    ICliWorkflowRun NextRun();

    /// <summary>Stops the workflow, moving <see cref="Status"/> to <see cref="CliWorkflowStatus.Stopped"/>.</summary>
    void Stop();

    /// <summary>Signals cooperative cancellation to the run currently executing, via <see cref="CancellationToken"/>.</summary>
    void InterruptCurrentRun();
}