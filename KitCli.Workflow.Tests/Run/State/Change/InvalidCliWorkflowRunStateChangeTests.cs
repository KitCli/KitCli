using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

public class InvalidCliWorkflowRunStateChangeTests : CliWorkflowRunStateTests
{
    public static IEnumerable<TestCaseData> InvalidStateChanges()
    {
        yield return new TestCaseData(
            Enumerable.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Created
        ).SetName("GivenStateIsCreated_WhenChangedToCreated_CannotBeChanged");
        
        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.Running
        ).SetName("GivenStateIsRunning_WhenChangedToRunning_CannotBeChanged");
        
        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.InvalidAsk },
            ClIWorkflowRunStateStatus.InvalidAsk
        ).SetName("GivenStateIsInvalidAsk_WhenChangedToInvalidAsk_CannotBeChanged");
        
        yield return new TestCaseData(
            Enumerable.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Exceptional
        ).SetName("GivenStateIsCreated_WhenChangedToExceptional_CannotBeChanged");
        
        yield return new TestCaseData(
            Enumerable.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Finished
        ).SetName("GivenStateIsCreated_WhenChangedToFinished_CannotBeChanged");
        
        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.Exceptional },
            ClIWorkflowRunStateStatus.Exceptional
        ).SetName("GivenStateIsExceptional_WhenChangedToExceptional_CannotBeChanged");
        
        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedFinalOutcome },
            ClIWorkflowRunStateStatus.ReachedFinalOutcome
        ).SetName("GivenStateIsAchievedOutcome_WhenChangedToAchievedOutcome_CannotBeChanged");
        
        yield return new TestCaseData(
            new []
            {
                ClIWorkflowRunStateStatus.Running,
                ClIWorkflowRunStateStatus.ReachedReusableOutcome,
                ClIWorkflowRunStateStatus.Running,
                ClIWorkflowRunStateStatus.ReachedFinalOutcome,
                ClIWorkflowRunStateStatus.Finished
            },
            ClIWorkflowRunStateStatus.Finished
        ).SetName("GivenStateIsFinished_WhenChangedToFinished_CannotBeChanged");
    }
    
    [TestCaseSource(nameof(InvalidStateChanges))]
    public void GivenStateIs_WhenChangedTo_CannotBeChanged(IEnumerable<ClIWorkflowRunStateStatus> priorStates, ClIWorkflowRunStateStatus stateToChangeTo)
    {
        // Arrange
        var state = GetPreparedState(priorStates);
        
        // Act & Assert
        Assert.Throws<ImpossibleStateChangeException>(() => state.ChangeTo(stateToChangeTo));
    }
}