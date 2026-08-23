using System.Diagnostics;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;

namespace KitCli.Workflow.Abstractions;

/// <summary>
/// The finite state machine tracking a single <see cref="ICliWorkflowRun"/>: an append-only
/// history of status changes, enforced against a fixed table of legal from/to transitions.
/// </summary>
public interface ICliWorkflowRunState
{
    /// <summary>Tracks elapsed time since the run started, used to timestamp each state change.</summary>
    Stopwatch Stopwatch { get; }

    /// <summary>The full, append-only history of every status change this run has undergone.</summary>
    List<ICliWorkflowRunStateChange> Changes { get; }

    /// <summary>Determines whether the run's history includes ever having changed to any of the given statuses.</summary>
    /// <param name="oneOfStatuses">The statuses to check for.</param>
    /// <returns><c>true</c> if the run has ever reached any of <paramref name="oneOfStatuses"/>; otherwise <c>false</c>.</returns>
    bool WasChangedTo(params ClIWorkflowRunStateStatus[] oneOfStatuses);

    /// <summary>Returns every state change in this run's history that carried outcomes.</summary>
    /// <returns>The outcome-carrying state changes, in the order they occurred.</returns>
    List<IOutcomeCliWorkflowRunStateChange> AllOutcomeStateChanges();

    /// <summary>
    /// Changes the run's status to <paramref name="statusToChangeTo"/>, recording the transition
    /// in <see cref="Changes"/>.
    /// </summary>
    /// <param name="statusToChangeTo">The status to transition to.</param>
    /// <exception cref="ImpossibleStateChangeException">
    /// The transition from the current status to <paramref name="statusToChangeTo"/> is not listed
    /// as legal.
    /// </exception>
    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo);

    /// <summary>
    /// Changes the run's status to <paramref name="statusToChangeTo"/>, recording the transition
    /// along with the instruction that triggered it.
    /// </summary>
    /// <param name="statusToChangeTo">The status to transition to.</param>
    /// <param name="instruction">The instruction that triggered this transition.</param>
    /// <exception cref="ImpossibleStateChangeException">
    /// The transition from the current status to <paramref name="statusToChangeTo"/> is not listed
    /// as legal.
    /// </exception>
    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, Instruction instruction);

    /// <summary>
    /// Changes the run's status to <paramref name="statusToChangeTo"/>, recording the transition
    /// along with the outcomes that triggered it.
    /// </summary>
    /// <param name="statusToChangeTo">The status to transition to.</param>
    /// <param name="outcomes">The outcomes that triggered this transition.</param>
    /// <exception cref="ImpossibleStateChangeException">
    /// The transition from the current status to <paramref name="statusToChangeTo"/> is not listed
    /// as legal.
    /// </exception>
    void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, Outcome[] outcomes);
}