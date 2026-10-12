using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State;

[TestFixture]
public class CliWorkflowRunStateWasLastChangedToTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenNoChanges_WhenWasLastChangedTo_ReturnsFalse()
    {
        // Arrange
        var state = GetPreparedState([]);

        // Act & Assert
        Assert.That(state.WasLastChangedTo(ClIWorkflowRunStateStatus.Running), Is.False);
    }

    [Test]
    public void GivenState_WhenWasLastChangedToLatestStatus_ReturnsTrue()
    {
        // Arrange
        var state = GetPreparedState(
        [
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
        ]);

        // Act & Assert
        Assert.That(state.WasLastChangedTo(ClIWorkflowRunStateStatus.Exceptional), Is.True);
    }

    [Test]
    public void GivenState_WhenWasLastChangedToEarlierStatus_ReturnsFalse()
    {
        // Arrange
        var state = GetPreparedState(
        [
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
        ]);

        // Act & Assert
        Assert.That(state.WasLastChangedTo(ClIWorkflowRunStateStatus.Running), Is.False);
    }
}
