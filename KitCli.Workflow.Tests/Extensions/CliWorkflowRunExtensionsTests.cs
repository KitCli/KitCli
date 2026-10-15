using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Extensions;
using KitCli.Workflow.Run.State;
using KitCli.Workflow.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Extensions;

[TestFixture]
public class CliWorkflowRunExtensionsTests
{
    [Test]
    public void GivenNoRuns_WhenAllSessionOutcomes_ThenNoneAreReturned()
    {
        // Arrange
        List<ICliWorkflowRun> runs = [];

        // Act
        var sessionOutcomes = runs.AllSessionOutcomes();

        // Assert
        Assert.That(sessionOutcomes, Is.Empty);
    }

    [Test]
    public void GivenRunWithSessionAndRunOutcomes_WhenAllSessionOutcomes_ThenOnlyTheSessionOutcomesAreReturned()
    {
        // Arrange
        var sessionOutcome = new TestSessionOutcome("prod");
        var runOutcome = new FinalSayOutcome("Using prod.");

        var state = new CliWorkflowRunState();
        state.ChangeTo(ClIWorkflowRunStateStatus.Running);
        state.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [sessionOutcome, runOutcome]);

        var run = new TestCliWorkflowRun(state);
        List<ICliWorkflowRun> runs = [run];

        // Act
        var sessionOutcomes = runs.AllSessionOutcomes();

        // Assert
        Assert.That(sessionOutcomes, Is.EqualTo(new Outcome[] { sessionOutcome }).AsCollection);
    }

    [Test]
    public void GivenLastRunWasHandedAnOlderRunsSessionOutcome_WhenAllSessionOutcomes_ThenEachIsReturnedOnce()
    {
        // Arrange
        var olderSessionOutcome = new TestSessionOutcome("prod");
        var newerSessionOutcome = new TestSessionOutcome("staging");

        var olderState = new CliWorkflowRunState();
        olderState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        olderState.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [olderSessionOutcome]);

        var lastState = new CliWorkflowRunState([olderSessionOutcome]);
        lastState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        lastState.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [newerSessionOutcome]);

        var olderRun = new TestCliWorkflowRun(olderState);
        var lastRun = new TestCliWorkflowRun(lastState);
        List<ICliWorkflowRun> runs = [olderRun, lastRun];

        // Act
        var sessionOutcomes = runs.AllSessionOutcomes();

        // Assert
        var expectedOutcomes = new Outcome[] { olderSessionOutcome, newerSessionOutcome };

        Assert.That(sessionOutcomes, Is.EqualTo(expectedOutcomes).AsCollection);
    }
}
