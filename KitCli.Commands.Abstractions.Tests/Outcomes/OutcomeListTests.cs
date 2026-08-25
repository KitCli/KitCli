using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Outcomes;

/// <summary>
/// Covers the two ways a handler chains to a command: by type, leaving construction to that command's
/// factory, and by instance, for a command that takes its data by constructor. The rest of
/// <see cref="OutcomeList"/>'s builder methods have no tests yet.
/// </summary>
// TODO: Just put it in OutcomeListTests?
[TestFixture]
public class OutcomeListTests
{
    private record TestNextCliCommand : CliCommand;

    [Test]
    public void GivenCommandType_WhenByMovingToCommand_ThenAppendsOutcomeCarryingThatTypeAndNoInstance()
    {
        // Act
        var outcomes = new OutcomeList()
            .ByMovingToCommand<TestNextCliCommand>()
            .End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new SpecifiedNextCliCommandOutcome(typeof(TestNextCliCommand))
        }).AsCollection);
    }

    [Test]
    public void GivenCommandInstance_WhenByMovingToCommand_ThenAppendsOutcomeCarryingThatInstance()
    {
        // Arrange
        var nextCommand = new TestNextCliCommand();

        // Act
        var outcomes = new OutcomeList()
            .ByMovingToCommand(nextCommand)
            .End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new ProvidedNextCliCommandOutcome(nextCommand)
        }).AsCollection);
    }

    [Test]
    public void GivenEitherOverload_WhenByMovingToCommand_ThenBothCarryAReusableOutcomeUnderTheSameBaseType()
    {
        // Act
        var outcomes = new OutcomeList()
            .ByMovingToCommand<TestNextCliCommand>()
            .ByMovingToCommand(new TestNextCliCommand())
            .End();

        // Assert - CliWorkflowRun finds the next command by the shared base, so both overloads must carry it.
        Assert.Multiple(() =>
        {
            Assert.That(outcomes, Is.All.InstanceOf<NextCliCommandOutcome>());
            Assert.That(outcomes.Select(outcome => outcome.IsReusable), Is.All.True);
        });
    }

    [Test]
    public void GivenTwoChainsToTheSameCommandType_WhenByMovingToCommand_ThenEachIsADistinctOutcomeObject()
    {
        // Act
        var outcomes = new OutcomeList()
            .ByMovingToCommand<TestNextCliCommand>()
            .ByMovingToCommand<TestNextCliCommand>()
            .End();

        // Assert - #152 selects the next command by outcome identity, which needs the two to stay separable.
        Assert.That(outcomes[0], Is.Not.SameAs(outcomes[1]));
    }
}
