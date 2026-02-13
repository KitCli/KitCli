using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Run.State.Change;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

[TestFixture]
public class OutcomeCliWorkflowStateChangeTests : CliWorkflowRunStateTests
{
    [Test]
    public void GivenOutcomeInWrongState_WhenChangeTo_CannotChangeState()
    {
        // Arrange
        var priorStatuses = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
            ClIWorkflowRunStateStatus.Finished
        };

        var state = GetPreparedState(priorStatuses);

        var outcome = new NothingOutcome();
        
        // Act & Assert
        Assert.Throws<ImpossibleStateChangeException>(() => state.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [outcome]));
    }

    [Test]
    public void GivenOutcomeInCorrectState_WhenChangeTo_CanChangeState()
    {
        // Arrange
        var priorStatuses = new[]
        {
            ClIWorkflowRunStateStatus.Running
        };

        var state = GetPreparedState(priorStatuses);

        var outcome = new NothingOutcome();
        
        // Act
        state.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [outcome]);
        
        // Assert
        var lastChange = state.Changes.Last();
        
        Assert.That(lastChange, Is.InstanceOf<OutcomeCliWorkflowRunStateChange>());
        
        var outcomeStateChange = (OutcomeCliWorkflowRunStateChange)lastChange;
        
        Assert.That(outcomeStateChange.Outcomes, Is.EqualTo([outcome]).AsCollection);
    }
}