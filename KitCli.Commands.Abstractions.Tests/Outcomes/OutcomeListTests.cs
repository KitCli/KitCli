using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Commands.Abstractions.Arguments;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Outcomes;

/// <summary>
/// Covers every <see cref="OutcomeList"/> builder method: each appends one outcome of the right kind,
/// carrying what it was given, in call order.
/// </summary>
[TestFixture]
public class OutcomeListTests
{
    [Test]
    public void GivenOutcome_WhenByResultingIn_ThenAppendsIt()
    {
        // Arrange
        var outcome = new NothingOutcome();

        // Act
        var outcomes = new OutcomeList().ByResultingIn(outcome).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { outcome }).AsCollection);
    }

    [Test]
    public void GivenSeveralOutcomes_WhenByResultingIn_ThenAppendsThemInOrder()
    {
        // Arrange
        var first = new SayOutcome("first");
        var second = new NothingOutcome();

        // Act
        var outcomes = new OutcomeList().ByResultingIn(first, second).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { first, second }).AsCollection);
    }

    [Test]
    public void GivenMessage_WhenBySaying_ThenAppendsSayOutcome()
    {
        // Act
        var outcomes = new OutcomeList().BySaying("hello").End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new SayOutcome("hello") }).AsCollection);
    }

    [Test]
    public void GivenSeveralMessages_WhenBySaying_ThenAppendsOneSayOutcomeEachInOrder()
    {
        // Act
        var outcomes = new OutcomeList().BySaying("first", "second").End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new SayOutcome("first"),
            new SayOutcome("second")
        }).AsCollection);
    }

    [Test]
    public void GivenTable_WhenByShowingTable_ThenAppendsTableOutcome()
    {
        // Arrange
        var table = new Table(["Column"], [[1]]);

        // Act
        var outcomes = new OutcomeList().ByShowingTable(table).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new TableOutcome(table) }).AsCollection);
    }

    [Test]
    public void GivenAggregator_WhenByAggregating_ThenAppendsAggregatorOutcome()
    {
        // Arrange
        var aggregator = new TestAggregator();

        // Act
        var outcomes = new OutcomeList().ByAggregating(aggregator).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new AggregatorOutcome<TestAggregate, TestAggregate>(aggregator)
        }).AsCollection);
    }

    [Test]
    public void GivenFilter_WhenByRememberingFilter_ThenAppendsAggregatorFilterOutcome()
    {
        // Arrange
        var filter = new AggregatorFilter("Name", "equals", "payee");

        // Act
        var outcomes = new OutcomeList().ByRememberingFilter(filter).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new AggregatorFilterOutcome(filter) }).AsCollection);
    }

    [Test]
    public void GivenPageSize_WhenByRememberingPageSize_ThenAppendsPageSizeOutcome()
    {
        // Act
        var outcomes = new OutcomeList().ByRememberingPageSize(25).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new PageSizeOutcome(25) }).AsCollection);
    }

    [Test]
    public void GivenPageNumber_WhenByRememberingPageNumber_ThenAppendsPageNumberOutcome()
    {
        // Act
        var outcomes = new OutcomeList().ByRememberingPageNumber(3).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new PageNumberOutcome(3) }).AsCollection);
    }

    [Test]
    public void GivenTableBuilder_WhenByRememberingHowToBuildTable_ThenAppendsTableBuilderOutcome()
    {
        // Arrange
        var tableBuilder = new TestTableBuilder();

        // Act
        var outcomes = new OutcomeList().ByRememberingHowToBuildTable(tableBuilder).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new TableBuilderOutcome<TestAggregate, TestAggregate>(tableBuilder)
        }).AsCollection);
    }

    // Asserted member by member rather than against a whole outcome: the record carries a List, and a
    // record's generated equality compares a List by reference, so two outcomes naming the same command
    // are never equal. Instruction has the same semantics.
    [Test]
    public void GivenCommandType_WhenByMovingToCommand_ThenAppendsOutcomeCarryingThatTypeAndNoArguments()
    {
        // Act
        var outcomes = new OutcomeList().ByMovingToCommand<TestNextCliCommand>().End();

        // Assert
        var outcome = outcomes.Single() as SpecifiedNextCliCommandOutcome;

        Assert.Multiple(() =>
        {
            Assert.That(outcome, Is.Not.Null);
            Assert.That(outcome!.SpecifiedCommandType, Is.EqualTo(typeof(TestNextCliCommand)));
            Assert.That(outcome.Arguments, Is.Empty);
        });
    }

    [Test]
    public void GivenArguments_WhenByMovingToCommand_ThenAppendsOutcomeCarryingThem()
    {
        // Arrange
        var limit = new NextCliCommandArgument<int>("limit", 10);

        // Act
        var outcomes = new OutcomeList().ByMovingToCommand<TestNextCliCommand>(limit).End();

        // Assert
        var outcome = outcomes.Single() as SpecifiedNextCliCommandOutcome;

        Assert.Multiple(() =>
        {
            Assert.That(outcome, Is.Not.Null);
            Assert.That(outcome!.SpecifiedCommandType, Is.EqualTo(typeof(TestNextCliCommand)));
            Assert.That(outcome.Arguments, Is.EqualTo(new AnonymousNextCliCommandArgument[] { limit }).AsCollection);
        });
    }

    [Test]
    public void GivenCommandInstance_WhenByMovingToCommand_ThenAppendsOutcomeCarryingThatInstance()
    {
        // Arrange
        var nextCommand = new TestNextCliCommand();

        // Act
        var outcomes = new OutcomeList().ByMovingToCommand(nextCommand).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new ProvidedNextCliCommandOutcome(nextCommand)
        }).AsCollection);
    }

    [Test]
    public void GivenCommandTypeWithNoParameterlessConstructor_WhenByMovingToCommand_ThenChainsToItAnyway()
    {
        // Assert - TCommand is constrained to CliCommand and nothing more. A new() constraint would read
        // as harmless and would exclude exactly the commands this overload exists for: the ones with
        // constructor arguments, which only a factory can supply. GetConstructor(Type.EmptyTypes) is the
        // same check AddCommandFactories makes before auto-registering a basic factory.
        Assert.That(typeof(TestParameterisedNextCliCommand).GetConstructor(Type.EmptyTypes), Is.Null);

        // Act
        var outcomes = new OutcomeList().ByMovingToCommand<TestParameterisedNextCliCommand>().End();

        // Assert
        var outcome = outcomes.Single() as SpecifiedNextCliCommandOutcome;

        Assert.That(outcome?.SpecifiedCommandType, Is.EqualTo(typeof(TestParameterisedNextCliCommand)));
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

    [Test]
    public void GivenReaction_WhenByReacting_ThenAppendsReactionOutcome()
    {
        // Arrange
        var reaction = new TestCliCommandReaction("a command ran");

        // Act
        var outcomes = new OutcomeList().ByReacting(reaction).End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new ReactionOutcome(reaction) }).AsCollection);
    }

    [Test]
    public void WhenByFinallyDoingNothing_ThenAppendsNothingOutcome()
    {
        // Act
        var outcomes = new OutcomeList().ByFinallyDoingNothing().End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new NothingOutcome() }).AsCollection);
    }

    [Test]
    public void GivenMessage_WhenByFinallySaying_ThenAppendsFinalSayOutcome()
    {
        // Act
        var outcomes = new OutcomeList().ByFinallySaying("done").End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new FinalSayOutcome("done") }).AsCollection);
    }

    [Test]
    public void WhenByFinallyNotFindingCommand_ThenAppendsCliCommandNotFoundOutcome()
    {
        // Act
        var outcomes = new OutcomeList().ByFinallyNotFindingCommand().End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[] { new CliCommandNotFoundOutcome() }).AsCollection);
    }

    [Test]
    public void GivenChainedCalls_WhenEnd_ThenReturnsEveryOutcomeInCallOrder()
    {
        // Act
        var outcomes = new OutcomeList()
            .BySaying("working")
            .ByRememberingPageSize(10)
            .ByFinallySaying("done")
            .End();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new SayOutcome("working"),
            new PageSizeOutcome(10),
            new FinalSayOutcome("done")
        }).AsCollection);
    }

    [Test]
    public async Task GivenChainedCalls_WhenEndAsync_ThenReturnsTheSameOutcomesAsEnd()
    {
        // Act
        var outcomes = await new OutcomeList()
            .BySaying("working")
            .ByFinallyDoingNothing()
            .EndAsync();

        // Assert
        Assert.That(outcomes, Is.EqualTo(new Outcome[]
        {
            new SayOutcome("working"),
            new NothingOutcome()
        }).AsCollection);
    }
}
