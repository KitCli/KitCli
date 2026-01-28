using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

[TestFixture]
public class ValidCliWorkflowRunStateChangeTests : CliWorkflowRunStateTests
{
    public static IEnumerable<TestCaseData> ValidStateChanges()
    {
        // Instruction does not validate, or empty ask.
        yield return new TestCaseData(
            Array.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.InvalidAsk
        ).SetName("GivenStateIsCreated_WhenChangeToInvalidAsk_CanBeChanged");
        
        // Is valid instruction
        yield return new TestCaseData(
            Array.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Running
        ).SetName("GivenStateIsCreated_WhenChangeToRunning_CanBeChanged");
        
        // NoCommandGeneratorException
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.InvalidAsk
        ).SetName("GivenStateIsRunning_WhenChangeToInvalidAsk_CanBeChanged");
        
        // Command handler responds to mediator with reusable outcome
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.ReachedReusableOutcome
        ).SetName("GivenStateIsRunning_WhenChangeToReachedReusableOutcome_CanBeChanged");
        
        // Command handler respond to mediator with a final outcome on second execute
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedReusableOutcome },
            ClIWorkflowRunStateStatus.Running
        ).SetName("GivenStateIsReachedReusableOutcome_WhenChangeToRunning_CanBeChanged");
        
        // Command handler responds to mediator with final outcome
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.ReachedFinalOutcome
        ).SetName("GivenStateIsRunning_WhenChangeToReachedFinalOutcome_CanBeChanged");
        
        // try/catch returns final outcome achieved
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedFinalOutcome },
            ClIWorkflowRunStateStatus.Finished
        ).SetName("GivenStateIsReachedFinalOutcome_WhenChangeToFinished_CanBeChanged");
        
        // Command handler failed.
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.Exceptional
        ).SetName("GivenStateIsRunning_WhenChangeToExceptional_CanBeChanged");
        
        // Exception handler finished.
        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.Exceptional },
            ClIWorkflowRunStateStatus.Finished
        ).SetName("GivenStateIsExceptional_WhenChangeToFinished_CanBeChanged");
    }
    
    [TestCaseSource(nameof(ValidStateChanges))]
    public void GivenStateIs_WhenChangeTo_CanBeChanged(IEnumerable<ClIWorkflowRunStateStatus> priorStates, ClIWorkflowRunStateStatus stateToChangeTo)
    {
        // Arrange
        var state = GetPreparedState(priorStates);
        
        // Act & Assert
        Assert.DoesNotThrow(() => state.ChangeTo(stateToChangeTo));
    }
    
    [TestCaseSource(nameof(ValidStateChanges))]
    public void GivenStateIsNotInitialized_WhenChangeToCreated_RecordsStateChange(ClIWorkflowRunStateStatus[] priorStates, ClIWorkflowRunStateStatus stateToChangeTo)
    {
        // Arrange
        var state = GetPreparedState(priorStates);
        
        // Act
        state.ChangeTo(stateToChangeTo);
        
        // Assert
        var priorStateChange = priorStates.Any() ? priorStates.Last() : ClIWorkflowRunStateStatus.Created;
        var stateChange = state.Changes.Last();
        
        Assert.That(stateChange, Is.Not.Null);
        Assert.That(stateChange.From, Is.EqualTo(priorStateChange));
        Assert.That(stateChange.To, Is.EqualTo(stateToChangeTo));
    }
}