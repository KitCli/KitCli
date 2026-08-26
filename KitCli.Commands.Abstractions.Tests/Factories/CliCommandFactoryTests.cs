using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Artefacts.RanCliCommand;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class CliCommandFactoryTests
{
    private TestCliCommandFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestCliCommandFactory();
    }

    [Test]
    public void GivenUnattachedFactory_WhenInstructionIsRead_ThenEmptyInstructionIsReturned()
    {
        // Assert
        Assert.That(_factory.Instruction, Is.EqualTo(Instruction.Empty));
    }

    [Test]
    public void GivenUnattachedFactory_WhenArtefactsAreRead_ThenNoArtefactsAreReturned()
    {
        // Assert
        Assert.That(_factory.Artefacts, Is.Empty);
    }

    [Test]
    public void GivenUnattachedFactory_WhenArgumentsAreRead_ThenItThrows()
    {
        // Assert
        Assert.That(() => _factory.GetArguments<string>().ToList(), Throws.Exception);
    }

    [Test]
    public void GivenUnattachedFactory_WhenArtefactsAreQueried_ThenItThrows()
    {
        // Assert
        Assert.That(() => _factory.GetArtefacts<int>().ToList(), Throws.Exception);
    }

    [Test]
    public void GivenUnattachedFactory_WhenSubCommandIsCalled_ThenItThrows()
    {
        // Assert
        Assert.That(() => _factory.SubCommandIs("page"), Throws.Exception);
    }

    [Test]
    public void GivenAttachedFactory_WhenAttachIsCalled_ThenTheSameFactoryIsReturned()
    {
        // Act
        var attached = _factory.Attach(Instruction.Empty, []);

        // Assert
        Assert.That(attached, Is.SameAs(_factory));
    }

    [Test]
    public void GivenAttachedInstruction_WhenInstructionIsRead_ThenTheAttachedInstructionIsReturned()
    {
        // Arrange
        var instruction = InstructionWith("page");

        // Act
        _factory.Attach(instruction, []);

        // Assert
        Assert.That(_factory.Instruction, Is.SameAs(instruction));
    }

    [Test]
    [TestCase("page", true)]
    [TestCase("size", false)]
    public void GivenInstructionWithSubCommand_WhenSubCommandIsCalled_ThenItMatchesOnlyThatName(
        string candidateSubCommandName,
        bool expectedMatch)
    {
        // Arrange
        _factory.Attach(InstructionWith("page"), []);

        // Act
        var matches = _factory.SubCommandIs(candidateSubCommandName);

        // Assert
        Assert.That(matches, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void GivenTypedArguments_WhenArgumentsAreRead_ThenOnlyThoseOfThatTypeAreReturned()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget"),
            new InstructionArgument<int>("count", 3)), []);

        // Act
        var arguments = _factory.GetArguments<string>().ToList();

        // Assert
        Assert.That(arguments.Select(argument => argument.Value), Is.EqualTo(new[] { "budget" }));
    }

    [Test]
    [TestCase("name", true)]
    [TestCase("missing", false)]
    [TestCase(null, true)]
    public void GivenTypedArgument_WhenAnyArgumentIsCalled_ThenTheNameFilterIsHonoured(
        string? argumentName,
        bool expectedMatch)
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget")), []);

        // Act
        var matches = _factory.AnyArgument<string>(argumentName);

        // Assert
        Assert.That(matches, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void GivenNoArgumentOfThatType_WhenAnyArgumentIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget")), []);

        // Act
        var matches = _factory.AnyArgument<int>(null);

        // Assert
        Assert.That(matches, Is.False);
    }

    [Test]
    public void GivenRepeatedArgumentName_WhenArgumentIsRead_ThenTheLastOneWins()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget"),
            new InstructionArgument<string>("name", "savings")), []);

        // Act
        var argument = _factory.GetArgument<string>("name");

        // Assert
        Assert.That(argument?.Value, Is.EqualTo("savings"));
    }

    [Test]
    public void GivenUnnamedArgumentLookup_WhenArgumentIsRead_ThenTheLastOfThatTypeIsReturned()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget"),
            new InstructionArgument<string>("label", "savings")), []);

        // Act
        var argument = _factory.GetArgument<string>(null);

        // Assert
        Assert.That(argument?.Value, Is.EqualTo("savings"));
    }

    [Test]
    public void GivenNoMatchingArgument_WhenArgumentIsRead_ThenNothingIsReturned()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget")), []);

        // Act
        var argument = _factory.GetArgument<string>("missing");

        // Assert
        Assert.That(argument, Is.Null);
    }

    [Test]
    public void GivenMatchingArgument_WhenRequiredArgumentIsRead_ThenItIsReturned()
    {
        // Arrange
        _factory.Attach(InstructionWith("page", new InstructionArgument<string>("name", "budget")), []);

        // Act
        var argument = _factory.GetRequiredArgument<string>("name");

        // Assert
        Assert.That(argument.Value, Is.EqualTo("budget"));
    }

    [Test]
    public void GivenNoMatchingArgument_WhenRequiredArgumentIsRead_ThenItThrows()
    {
        // Arrange
        _factory.Attach(InstructionWith("page"), []);

        // Assert
        Assert.That(() => _factory.GetRequiredArgument<string>("name"), Throws.Exception);
    }

    [Test]
    [TestCase("PageNumber", true)]
    [TestCase("Missing", false)]
    [TestCase(null, true)]
    public void GivenTypedArtefact_WhenAnyArtefactIsCalled_ThenTheNameFilterIsHonoured(
        string? artefactName,
        bool expectedMatch)
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageNumberArtefact(2)]);

        // Act
        var matches = _factory.AnyArtefact<int>(artefactName);

        // Assert
        Assert.That(matches, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void GivenNoArtefactOfThatType_WhenAnyArtefactIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageNumberArtefact(2)]);

        // Act
        var matches = _factory.AnyArtefact<string>(null);

        // Assert
        Assert.That(matches, Is.False);
    }

    [Test]
    public void GivenArtefactsSharingAType_WhenArtefactIsReadByName_ThenTheNamedOneIsReturned()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageNumberArtefact(2), new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetArtefact<int>(nameof(PageNumberArtefact.PageNumber));

        // Assert
        Assert.That(artefact?.Value, Is.EqualTo(2));
    }

    [Test]
    public void GivenArtefactsSharingAType_WhenArtefactIsReadUnnamed_ThenTheLastOneIsReturned()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageNumberArtefact(2), new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetArtefact<int>();

        // Assert
        Assert.That(artefact?.Value, Is.EqualTo(50));
    }

    [Test]
    public void GivenNoMatchingArtefact_WhenArtefactIsRead_ThenNothingIsReturned()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageNumberArtefact(2)]);

        // Act
        var artefact = _factory.GetArtefact<int>("Missing");

        // Assert
        Assert.That(artefact, Is.Null);
    }

    [Test]
    public void GivenMatchingArtefact_WhenRequiredArtefactIsRead_ThenItIsReturned()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetRequiredArtefact<int>(nameof(PageSizeArtefact.PageSize));

        // Assert
        Assert.That(artefact.Value, Is.EqualTo(50));
    }

    [Test]
    public void GivenNoMatchingArtefact_WhenRequiredArtefactIsRead_ThenItThrows()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, []);

        // Assert
        Assert.That(() => _factory.GetRequiredArtefact<int>(), Throws.Exception);
    }

    [Test]
    public void GivenAggregatorArtefact_WhenAggregatorArtefactIsRead_ThenItIsReturned()
    {
        // Arrange
        var aggregator = new TestAggregator();
        _factory.Attach(Instruction.Empty, [new AggregatorArtefact<TestAggregate, TestAggregate>(aggregator)]);

        // Act
        var artefact = _factory.GetAggregatorArtefact<TestAggregate, TestAggregate>();

        // Assert
        Assert.That(artefact?.Value, Is.SameAs(aggregator));
    }

    [Test]
    public void GivenAggregatorArtefact_WhenAggregatorArtefactIsReadByName_ThenTheNameFilterIsHonoured()
    {
        // Arrange
        _factory.Attach(Instruction.Empty,
            [new AggregatorArtefact<TestAggregate, TestAggregate>(new TestAggregator())]);

        // Act
        var artefact = _factory.GetAggregatorArtefact<TestAggregate, TestAggregate>("Missing");

        // Assert
        Assert.That(artefact, Is.Null);
    }

    [Test]
    public void GivenMatchingAggregatorArtefact_WhenRequiredAggregatorArtefactIsRead_ThenItIsReturned()
    {
        // Arrange
        var aggregator = new TestAggregator();
        _factory.Attach(Instruction.Empty, [new AggregatorArtefact<TestAggregate, TestAggregate>(aggregator)]);

        // Act
        var artefact = _factory.GetRequiredAggregatorArtefact<TestAggregate, TestAggregate>(nameof(TestAggregator));

        // Assert
        Assert.That(artefact.Value, Is.SameAs(aggregator));
    }

    [Test]
    public void GivenNoAggregatorArtefact_WhenRequiredAggregatorArtefactIsRead_ThenItThrows()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, []);

        // Assert
        Assert.That(() => _factory.GetRequiredAggregatorArtefact<TestAggregate, TestAggregate>(), Throws.Exception);
    }

    [Test]
    public void GivenTheCommandRan_WhenLastCommandWasIsCalled_ThenItMatches()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, [new RanCliCommandArtefact(new TestNextCliCommand())]);

        // Act
        var ran = _factory.LastCommandWas<TestNextCliCommand>();

        // Assert
        Assert.That(ran, Is.True);
    }

    [Test]
    public void GivenAnotherCommandRan_WhenLastCommandWasIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        _factory.Attach(Instruction.Empty,
            [new RanCliCommandArtefact(new TestParameterisedNextCliCommand("budget"))]);

        // Act
        var ran = _factory.LastCommandWas<TestNextCliCommand>();

        // Assert
        Assert.That(ran, Is.False);
    }

    [Test]
    public void GivenNoCommandRan_WhenLastCommandWasIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        _factory.Attach(Instruction.Empty, []);

        // Act
        var ran = _factory.LastCommandWas<TestNextCliCommand>();

        // Assert
        Assert.That(ran, Is.False);
    }

    private static Instruction InstructionWith(string subInstructionName,
        params AnonymousInstructionArgument[] arguments)
        => new("/", "test", subInstructionName, arguments.ToList());
}
