using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State;

[TestFixture]
public class CliWorkflowRunStateStopwatchTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenStateIsCreated_WhenChangeToRunning_StartsStopwatch()
    {
        // Arrange
        var state = GetPreparedState([]);
        
        // Act
        state.ChangeTo(ClIWorkflowRunStateStatus.Running);
        
        // Assert
        Assert.That(state.Stopwatch.IsRunning, Is.True);
    }

    [Test]
    public void GivenStateIsRunning_WhenChangeToFinished_StopsStopwatch()
    {
        // Arrange
        var state = GetPreparedState([ClIWorkflowRunStateStatus.InvalidAsk]);
        
        // Act
        state.ChangeTo(ClIWorkflowRunStateStatus.Finished);
        
        // Assert
        Assert.That(state.Stopwatch.IsRunning, Is.False);
    }
}