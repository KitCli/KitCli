using KitCli.Workflow.Abstractions;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run.State.Change;

[TestFixture]
public class ValidCliWorkflowRunStateChangeTests : CliWorkflowRunStateTests
{
    public static IEnumerable<TestCaseData> ValidStateChanges()
    {
        yield return new TestCaseData(
            Array.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.InvalidAsk
        )
        .SetName("GivenStateIsCreated_WhenChangeToInvalidAsk_CanBeChanged")
        .SetDescription("Instruction does not validate, or empty ask.");

        yield return new TestCaseData(
            Array.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsCreated_WhenChangeToRunning_CanBeChanged")
        .SetDescription("Is valid instruction.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.InvalidAsk
        )
        .SetName("GivenStateIsRunning_WhenChangeToInvalidAsk_CanBeChanged")
        .SetDescription("NoCommandGeneratorException.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.ReachedReusableOutcome
        )
        .SetName("GivenStateIsRunning_WhenChangeToReachedReusableOutcome_CanBeChanged")
        .SetDescription("Command handler responds to mediator with reusable outcome.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedReusableOutcome },
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsReachedReusableOutcome_WhenChangeToRunning_CanBeChanged")
        .SetDescription("Command handler respond to mediator with a final outcome on second execute.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.ReachedFinalOutcome
        )
        .SetName("GivenStateIsRunning_WhenChangeToReachedFinalOutcome_CanBeChanged")
        .SetDescription("Command handler responds to mediator with final outcome.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedFinalOutcome },
            ClIWorkflowRunStateStatus.Finished
        )
        .SetName("GivenStateIsReachedFinalOutcome_WhenChangeToFinished_CanBeChanged")
        .SetDescription("try/catch returns final outcome achieved.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.Exceptional
        )
        .SetName("GivenStateIsRunning_WhenChangeToExceptional_CanBeChanged")
        .SetDescription("Command handler failed.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.Exceptional },
            ClIWorkflowRunStateStatus.Finished
        )
        .SetName("GivenStateIsExceptional_WhenChangeToFinished_CanBeChanged")
        .SetDescription("Exception handler finished.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.MovePastAsk
        )
        .SetName("GivenStateIsRunning_WhenChangeToMovePastAsk_CanBeChanged")
        .SetDescription("Command handler responds to mediator with a NextCliCommandOutcome.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.MovePastAsk },
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsMovePastAsk_WhenChangeToRunning_CanBeChanged")
        .SetDescription("MoveToNext resolves and re-enters Running.");

        yield return new TestCaseData(
            new[] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.MovePastAsk },
            ClIWorkflowRunStateStatus.InvalidMovePastAsk
        )
        .SetName("GivenStateIsMovePastAsk_WhenChangeToInvalidMovePastAsk_CanBeChanged")
        .SetDescription("MoveToNext called with nothing to continue with.");

        yield return new TestCaseData(
            new[]
            {
                ClIWorkflowRunStateStatus.Running,
                ClIWorkflowRunStateStatus.MovePastAsk,
                ClIWorkflowRunStateStatus.InvalidMovePastAsk
            },
            ClIWorkflowRunStateStatus.Finished
        )
        .SetName("GivenStateIsInvalidMovePastAsk_WhenChangeToFinished_CanBeChanged")
        .SetDescription("Invalid move-past-ask finishes the run.");
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
