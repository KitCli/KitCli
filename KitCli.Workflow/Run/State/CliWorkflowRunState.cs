using System.Diagnostics;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Abstractions.Run.State.Change;
using KitCli.Workflow.Run.State.Change;

namespace KitCli.Workflow.Run.State;

public class CliWorkflowRunState : ICliWorkflowRunState
{
    public Stopwatch Stopwatch { get; }= new Stopwatch();
    public List<ICliWorkflowRunStateChange> Changes { get; } = [];

    public bool WasChangedTo(ClIWorkflowRunStateStatus status)
     =>  Changes.Any(change => change.To == status);
    
    public bool WasChangedTo(params ClIWorkflowRunStateStatus[] oneOfStatuses)
        => Changes.Any(change => oneOfStatuses.Contains(change.To));

    // TODO: Why not just use (ReachedReusableOutcome)?
    public bool WasChangedToReusableOutcome()
    {
        var lastOutcomeChange = Changes
            .OfType<OutcomeCliWorkflowRunStateChange>()
            .LastOrDefault();

        var includesReusableOutcome = lastOutcomeChange?
            .Outcomes
            .Any(p => p.IsReusable);

        return includesReusableOutcome ?? false;
    }
    
    public List<IOutcomeCliWorkflowRunStateChange> AllOutcomeStateChanges() 
        => Changes
            .OfType<IOutcomeCliWorkflowRunStateChange>()
            .ToList();

    public void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo)
    {
        var priorState = CanChangeTo(statusToChangeTo);
        
        UpdateStopwatch(statusToChangeTo);

        var stateChange = new CliWorkflowRunStateChange(
            Stopwatch.Elapsed,
            priorState, 
            statusToChangeTo);
        
        Changes.Add(stateChange);
    }

    public void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, CliInstruction instruction)
    {
        var priorState = CanChangeTo(statusToChangeTo);
        
        UpdateStopwatch(statusToChangeTo);
        
        var stateChange = new InstructionCliWorkflowRunStateChange(
            Stopwatch.Elapsed,
            priorState, 
            statusToChangeTo,
            instruction);
        
        Changes.Add(stateChange);
    }
    
    public void ChangeTo(ClIWorkflowRunStateStatus statusToChangeTo, Outcome[] outcomes)
    {
        var priorState = CanChangeTo(statusToChangeTo);
        
        UpdateStopwatch(statusToChangeTo);
        
        var stateChange = new OutcomeCliWorkflowRunStateChange(
            Stopwatch.Elapsed,
            priorState, 
            statusToChangeTo,
            outcomes);
        
        Changes.Add(stateChange);
    }

    private ClIWorkflowRunStateStatus CanChangeTo(ClIWorkflowRunStateStatus stateStatusToChangeTo)
    {
        var mostRecentState = Changes.LastOrDefault();
        var priorState = mostRecentState?.To ?? ClIWorkflowRunStateStatus.Created;
        
        // Can chnge from most recently changed to, to new state to change to.
        var possibleStateChange = PossibleStateChanges
            .Any(cliWorkflowRunStateChange =>
                cliWorkflowRunStateChange.IfStartedAt == priorState && 
                cliWorkflowRunStateChange.CanMoveTo == stateStatusToChangeTo);

        if (!possibleStateChange)
        {
            throw new ImpossibleStateChangeException($"Invalid state change: {priorState} > {stateStatusToChangeTo}");
        }
        
        return priorState;
    }

    private void UpdateStopwatch(ClIWorkflowRunStateStatus stateStatusToChangeTo)
    {
        if (stateStatusToChangeTo == ClIWorkflowRunStateStatus.Running)
        {
            Stopwatch.Start();
        }

        if (stateStatusToChangeTo == ClIWorkflowRunStateStatus.Finished)
        {
            Stopwatch.Stop();
        }
    }

    /// <summary>
    /// Stops a CliWorkflowRun from being re-eexecuted for another command.
    /// </summary>
    private static readonly List<PossibleCliWorkflowRunStateChange> PossibleStateChanges =
    [
        new(ClIWorkflowRunStateStatus.Created, ClIWorkflowRunStateStatus.InvalidAsk),
        new(ClIWorkflowRunStateStatus.Created, ClIWorkflowRunStateStatus.Running),
        
        new(ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.InvalidAsk),
        new(ClIWorkflowRunStateStatus.InvalidAsk, ClIWorkflowRunStateStatus.Finished),
        
        new(ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.Exceptional),
        new(ClIWorkflowRunStateStatus.Exceptional, ClIWorkflowRunStateStatus.Finished),
        
        new(ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedReusableOutcome),
        new(ClIWorkflowRunStateStatus.ReachedReusableOutcome, ClIWorkflowRunStateStatus.Running),
        
        new(ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.MovePastAsk),
        new(ClIWorkflowRunStateStatus.MovePastAsk, ClIWorkflowRunStateStatus.Running),
        
        new(ClIWorkflowRunStateStatus.MovePastAsk, ClIWorkflowRunStateStatus.InvalidMovePastAsk),
        new(ClIWorkflowRunStateStatus.InvalidMovePastAsk, ClIWorkflowRunStateStatus.Finished),
        
        new(ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedFinalOutcome),
        new(ClIWorkflowRunStateStatus.ReachedFinalOutcome, ClIWorkflowRunStateStatus.Finished),
    ];
}