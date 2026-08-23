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
        )
        .SetName("GivenStateIsCreated_WhenChangedToCreated_CannotBeChanged")
        .SetDescription("Created is the implicit starting status, never a status to change to.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running },
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsRunning_WhenChangedToRunning_CannotBeChanged")
        .SetDescription("A run cannot re-enter Running from Running directly.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.InvalidAsk },
            ClIWorkflowRunStateStatus.InvalidAsk
        )
        .SetName("GivenStateIsInvalidAsk_WhenChangedToInvalidAsk_CannotBeChanged")
        .SetDescription("InvalidAsk's only legal next status is Finished.");

        yield return new TestCaseData(
            Enumerable.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Exceptional
        )
        .SetName("GivenStateIsCreated_WhenChangedToExceptional_CannotBeChanged")
        .SetDescription("A command can only fail once it has actually started running.");

        yield return new TestCaseData(
            Enumerable.Empty<ClIWorkflowRunStateStatus>(),
            ClIWorkflowRunStateStatus.Finished
        )
        .SetName("GivenStateIsCreated_WhenChangedToFinished_CannotBeChanged")
        .SetDescription("A run cannot finish before it has reached any terminal status.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.Exceptional },
            ClIWorkflowRunStateStatus.Exceptional
        )
        .SetName("GivenStateIsExceptional_WhenChangedToExceptional_CannotBeChanged")
        .SetDescription("Exceptional's only legal next status is Finished.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.ReachedFinalOutcome },
            ClIWorkflowRunStateStatus.ReachedFinalOutcome
        )
        .SetName("GivenStateIsAchievedOutcome_WhenChangedToAchievedOutcome_CannotBeChanged")
        .SetDescription("ReachedFinalOutcome's only legal next status is Finished.");

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
        )
        .SetName("GivenStateIsFinished_WhenChangedToFinished_CannotBeChanged")
        .SetDescription("Finished is terminal - there is no legal transition out of it.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.MovePastAsk },
            ClIWorkflowRunStateStatus.Finished
        )
        .SetName("GivenStateIsMovePastAsk_WhenChangedToFinished_CannotBeChanged")
        .SetDescription("MovePastAsk must go through InvalidMovePastAsk before it can finish.");

        yield return new TestCaseData(
            new []
            {
                ClIWorkflowRunStateStatus.Running,
                ClIWorkflowRunStateStatus.MovePastAsk,
                ClIWorkflowRunStateStatus.InvalidMovePastAsk,
                ClIWorkflowRunStateStatus.Finished
            },
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsFinishedViaInvalidMovePastAsk_WhenChangedToRunning_CannotBeChanged")
        .SetDescription("Finished reached via InvalidMovePastAsk is still terminal, same as any other path.");

        yield return new TestCaseData(
            new [] { ClIWorkflowRunStateStatus.Running, ClIWorkflowRunStateStatus.InvalidAsk },
            ClIWorkflowRunStateStatus.Running
        )
        .SetName("GivenStateIsInvalidAsk_WhenChangedToRunning_CannotBeChanged")
        .SetDescription("Regression guard for the crash caused by reusing a run stuck at InvalidAsk.");
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
