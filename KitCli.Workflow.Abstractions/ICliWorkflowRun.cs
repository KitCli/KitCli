using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Abstractions;

/// <summary>
/// A single execution arc within a workflow, tracking one interaction from an ask being received
/// through to the run finishing, across as many commands as it takes to reach a final outcome.
/// </summary>
public interface ICliWorkflowRun
{
    /// <summary>The run's current status and the full history of status changes leading to it.</summary>
    ICliWorkflowRunState State { get; }

    /// <summary>
    /// Parses, validates, and executes the command resolved from fresh user input, driving the
    /// run's state machine through the resulting transitions.
    /// </summary>
    /// <param name="ask">The raw user input to respond to, or <c>null</c>/empty to invalidate the run.</param>
    /// <returns>The outcomes produced by executing the resolved command.</returns>
    ValueTask<Outcome[]> RespondToAsk(string? ask);

    /// <summary>
    /// Continues this run into its next command using what a prior outcome already queued up,
    /// without waiting on a new ask (e.g. paging through a list).
    /// </summary>
    /// <returns>The outcomes produced by executing the queued-up next command.</returns>
    ValueTask<Outcome[]> MoveToNext();
}