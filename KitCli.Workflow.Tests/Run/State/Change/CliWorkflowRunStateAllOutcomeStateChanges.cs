using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

[TestFixture]
public class CliWorkflowRunStateAllOutcomeStateChanges : CliWorkflowRunStateTests
{
    [Test]
    public void GivenNoStateChanges_WhenAllOutcomeStateChangesCalled_ReturnsEmptyCollection()
    {
        // Arrange
        var state = GetPreparedState([]);

        // Act
        var outcomeStateChanges = state.AllOutcomeStateChanges();

        // Assert
        Assert.That(outcomeStateChanges, Is.Empty);
    }

    [Test]
    public void GivenNonOutcomeStateChanges_WhenAllOutcomeStateChangesCalled_ReturnsEmptyCollection()
    {
        // Arrange
        var priorStates = new List<ClIWorkflowRunStateStatus>
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.InvalidAsk,
            ClIWorkflowRunStateStatus.Finished
        };
        
        var state = GetPreparedState(priorStates);
        
        // Act
        var outcomeStateChanges = state.AllOutcomeStateChanges();
        
        // Assert
        Assert.That(outcomeStateChanges, Is.Empty);
    }
    
    [Test]
    public void GivenOnlyFinalOutcomeStateChanges_WhenAllOutcomeStateChangesCalled_ReturnsThatChange()
    {
        // Arrange
        var priorStates = new List<ClIWorkflowRunStateStatus>
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedFinalOutcome
        };
        
        var state = GetPreparedState(priorStates);
        
        // Act
        var outcomeStateChanges = state.AllOutcomeStateChanges();
        
        // Assert
        Assert.That(outcomeStateChanges, Is.Empty);
    }
    
    [Test]
    public void GivenReusableOutcomeStateChanges_WhenAllOutcomeStateChangesCalled_ReturnsThoseChanges()
    {
        // Arrange
        var priorStates = new List<ClIWorkflowRunStateStatus>
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedReusableOutcome,
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedFinalOutcome,
            ClIWorkflowRunStateStatus.Finished
        };
        
        var state = GetPreparedState(priorStates);
        
        // Act
        var outcomeStateChanges = state.AllOutcomeStateChanges();
        
        // Assert
        Assert.That(outcomeStateChanges, Has.Count.EqualTo(1));
    }
}