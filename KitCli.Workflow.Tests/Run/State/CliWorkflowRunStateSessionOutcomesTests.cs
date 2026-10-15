using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Run.State;
using KitCli.Workflow.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State;

[TestFixture]
public class CliWorkflowRunStateSessionOutcomesTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenSessionOutcomes_WhenStateIsCreated_ThenItsOnlyChangeCarriesThem()
    {
        // Arrange
        Outcome[] sessionOutcomes = [new TestSessionOutcome("prod")];

        // Act
        var state = new CliWorkflowRunState(sessionOutcomes);

        // Assert
        var onlyChange = state.AllOutcomeStateChanges().Single();

        Assert.That(state.Changes, Has.Count.EqualTo(1));
        Assert.That(onlyChange.Outcomes, Is.EqualTo(sessionOutcomes).AsCollection);
    }

    [Test]
    public void GivenSessionOutcomes_WhenStateIsCreated_ThenItStaysCreated()
    {
        // Arrange
        Outcome[] sessionOutcomes = [new TestSessionOutcome("prod")];

        // Act
        var state = new CliWorkflowRunState(sessionOutcomes);

        // Assert
        var onlyChange = state.Changes.Single();

        Assert.That(onlyChange.From, Is.EqualTo(ClIWorkflowRunStateStatus.Created));
        Assert.That(onlyChange.To, Is.EqualTo(ClIWorkflowRunStateStatus.Created));
    }

    [Test]
    public void GivenStateCreatedWithSessionOutcomes_WhenChangedToRunning_ThenTheChangeIsAllowed()
    {
        // Arrange
        Outcome[] sessionOutcomes = [new TestSessionOutcome("prod")];

        var state = new CliWorkflowRunState(sessionOutcomes);

        // Act
        state.ChangeTo(ClIWorkflowRunStateStatus.Running);

        // Assert
        Assert.That(state.WasLastChangedTo(ClIWorkflowRunStateStatus.Running), Is.True);
    }
}
