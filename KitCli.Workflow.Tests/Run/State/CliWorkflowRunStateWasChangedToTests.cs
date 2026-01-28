using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State;

[TestFixture]
public class CliWorkflowRunStateWasChangedToTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenState_WhenWasCalledWithPriorStatus_ReturnsTrue()
    {
        // Arrange
        var priorStatuses = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
        };
        
        var state = GetPreparedState(priorStatuses);
        
        // Act & Assert
        foreach (var priorStatus in priorStatuses)
        {
            Assert.That(state.WasChangedTo(priorStatus), Is.True);
        }
    }

    [Test]
    public void GivenState_WhenWasCalledWithNonPriorStatus_ReturnsFalse()
    {
        // Arrange
        var priorStatuses = new[]
        {
            ClIWorkflowRunStateStatus.Running
        };
        
        var state = GetPreparedState(priorStatuses);
        
        // Act & Assert
        Assert.That(state.WasChangedTo(ClIWorkflowRunStateStatus.Finished), Is.False);
        Assert.That(state.WasChangedTo(ClIWorkflowRunStateStatus.Exceptional), Is.False);
    }
}