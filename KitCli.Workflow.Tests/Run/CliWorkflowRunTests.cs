using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run;
using KitCli.Workflow.Run.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run;

[TestFixture]
public class CliWorkflowRunTests
{
    [CliNextCommandIs("next", "Show the next page.")]
    [CliNextCommandIs("prev", "Show the previous page.")]
    private record TestReusableCommand : CliCommand;

    private record TestChainingCliCommand : CliCommand;

    private record TestChainedToCliCommand(string Name) : CliCommand;

    private record TestFactoryBuiltCliCommand : CliCommand;

    private static readonly Instruction OriginatingInstruction =
        new("/", "some-valid-ask", "with-detail", []);

    private static readonly IOptions<InstructionSettings> DefaultInstructionSettings =
        Options.Create(new InstructionSettings());

    private abstract record TestAggregate;

    private record TestListAggregator() : Aggregator<TestAggregate, TestAggregate>([])
    {
        protected override IEnumerable<TestAggregate> DoAggregation(IEnumerable<TestAggregate> source) => new List<TestAggregate>(source);
    }

    private CliWorkflowRunState _cliWorkflowRunState;
    private Mock<IServiceScope> _scope;
    private Mock<IInstructionParser> _cliInstructionParser;
    private Mock<IInstructionValidator> _cliInstructionValidator;
    private Mock<ICliWorkflowCommandProvider> _cliWorkflowCommandProvider;
    private Mock<ISender> _sender;
    private Mock<IPublisher> _publisher;
    private CliWorkflowRun _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _cliWorkflowRunState = new CliWorkflowRunState();
        _scope = new Mock<IServiceScope>();
        _cliInstructionParser = new Mock<IInstructionParser>();
        _cliInstructionValidator = new Mock<IInstructionValidator>();
        _cliWorkflowCommandProvider = new Mock<ICliWorkflowCommandProvider>();
        _sender = new Mock<ISender>();
        _publisher = new Mock<IPublisher>();

        _classUnderTest = new CliWorkflowRun(
            _cliWorkflowRunState,
            _scope.Object,
            _cliInstructionParser.Object,
            _cliInstructionValidator.Object,
            _cliWorkflowCommandProvider.Object,
            DefaultInstructionSettings,
            _sender.Object,
            _publisher.Object
            );
    }
    
    [Test]
    public async Task GivenInvalidAsk_WhenRespondToAsk_ReturnsNothingOutcome()
    {
        // Arrange
        var ask = string.Empty;
        
        // Act
        var outcomes = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var firstOutcome = outcomes.FirstOrDefault();
        
        Assert.That(firstOutcome, Is.InstanceOf<NothingOutcome>());
    }
    
    [Test]
    public async Task GivenInvalidAsk_WhenRespondToAsk_ChangesStateToInvalidAskThenFinished()
    {
        // Arrange
        var ask = string.Empty;

        // Act
        _ = await _classUnderTest.RespondToAsk(ask);

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.InvalidAsk,
            ClIWorkflowRunStateStatus.Finished
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenInstructionParserFails_WhenRespondToAsk_StateChangeBeforeFinishIsInvalidAsk()
    {
        // Arrange
        var ask = "some valid ask";

        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(false);

        // Act
        _ = await _classUnderTest.RespondToAsk(ask);

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.InvalidAsk,
            ClIWorkflowRunStateStatus.Finished
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }
    
    [Test]
    public async Task GivenCommandProviderFails_WhenRespondToAsk_StateChangeBeforeFinishIsInvalidAsk()
    {
        // Arrange
        var ask = "some valid ask";
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(new Instruction("prefix", "name", null, []));
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Throws<NoCommandGeneratorException>();
        
        // Act
        _ = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.InvalidAsk,
            ClIWorkflowRunStateStatus.Finished
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To)
            .ToList();

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenCommandExecutionFails_WhenRespondToAsk_StateChangeBeforeFinishIsExceptional()
    {
        // Arrange
        var ask = "/some-valid-ask";
        
        var instruction = new Instruction("/", "some-valid-ask", null, []);
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new CliCommand());

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .Throws<Exception>();
        
        // Act
        _ = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
            ClIWorkflowRunStateStatus.Finished
        };
        
        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenValidAskWithFinalOutcome_WhenRespondToAsk_StateChangesToFinished()
    {
        // Arrange
        var ask = "some valid ask";
        
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var nothingOutcome = new NothingOutcome();
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new CliCommand());

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([nothingOutcome]);
        
        // Act
        var resultingOutcomes = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedFinalOutcome,
            ClIWorkflowRunStateStatus.Finished
        };
        
        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
        Assert.That(resultingOutcomes.Length, Is.EqualTo(2));
        
        var resultingOutcome = resultingOutcomes[1];
        Assert.That(resultingOutcome, Is.EqualTo(nothingOutcome));
    }
    
    [Test]
    public async Task GivenValidAskWithReusableOutcome_WhenRespondToAsk_StateChangesToReachedReusableOutcome()
    {
        // Arrange
        var ask = "some valid ask";
        
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var aggregator = new TestListAggregator();
        var outcome = new AggregatorOutcome<TestAggregate, TestAggregate>(aggregator);
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new CliCommand());

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([outcome]);
        
        // Act
        var resultingOutcomes = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedReusableOutcome,
        };
        
        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
        Assert.That(resultingOutcomes.Length, Is.EqualTo(2));
        
        var resultingOutcome = resultingOutcomes[1];
        Assert.That(resultingOutcome, Is.EqualTo(outcome));
    }

    [Test]
    public async Task GivenCommandProviderFailsAfterReachingReusableOutcome_WhenRespondToAsk_LeavesRunAtReusableOutcome()
    {
        // Arrange
        var ask = "some valid ask";
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var aggregator = new TestListAggregator();
        var reusableOutcome = new AggregatorOutcome<TestAggregate, TestAggregate>(aggregator);

        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);

        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);

        _cliWorkflowCommandProvider
            .SetupSequence(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new CliCommand())
            .Throws<NoCommandGeneratorException>();

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([reusableOutcome]);

        // Act - first ask reaches the reusable checkpoint.
        _ = await _classUnderTest.RespondToAsk(ask);

        // Act - second ask fails to resolve a command.
        var secondOutcomes = await _classUnderTest.RespondToAsk(ask);

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedReusableOutcome,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
        Assert.That(secondOutcomes.FirstOrDefault(), Is.InstanceOf<NothingOutcome>());
    }

    [Test]
    public async Task GivenCommandProviderFailsAfterReachingReusableOutcomeWithDeclaredNextCommands_WhenRespondToAsk_SuggestsThem()
    {
        // Arrange
        var ask = "some valid ask";
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var aggregator = new TestListAggregator();
        var reusableOutcome = new AggregatorOutcome<TestAggregate, TestAggregate>(aggregator);

        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);

        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);

        _cliWorkflowCommandProvider
            .SetupSequence(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new TestReusableCommand())
            .Throws<NoCommandGeneratorException>();

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([reusableOutcome]);

        // Act - first ask reaches the reusable checkpoint.
        _ = await _classUnderTest.RespondToAsk(ask);

        // Act - second ask fails to resolve a command.
        var secondOutcomes = await _classUnderTest.RespondToAsk(ask);

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedReusableOutcome,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);

        var expectedOutcomes = new Outcome[]
        {
            new SuggestionOutcome("/next", "Show the next page."),
            new SuggestionOutcome("/prev", "Show the previous page."),
        };

        Assert.That(secondOutcomes, Is.EqualTo(expectedOutcomes).AsCollection);
    }

    [Test]
    public async Task GivenValidAskWithNextCliCommandOutcome_WhenRespondToAsk_StateChangesToMovePastAsk()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");

        // Act
        await RespondToAskWithNextCommands(nextCommand);

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.MovePastAsk,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenProvidedNextCommand_WhenMoveToNext_SendsThatCommand()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");

        await RespondToAskWithNextCommands(nextCommand);

        _sender
            .Setup(mediator => mediator.Send(nextCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        _sender.Verify(
            mediator => mediator.Send(nextCommand, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GivenProvidedNextCommand_WhenMoveToNext_ReturnsRanCommandOutcomeThenItsOutcomes()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");
        var nothingOutcome = new NothingOutcome();

        await RespondToAskWithNextCommands(nextCommand);

        _sender
            .Setup(mediator => mediator.Send(nextCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync([nothingOutcome]);

        // Act
        var resultingOutcomes = await _classUnderTest.MoveToNext();

        // Assert
        var expectedOutcomes = new Outcome[]
        {
            new RanCliCommandOutcome(nextCommand),
            nothingOutcome,
        };

        Assert.That(resultingOutcomes, Is.EqualTo(expectedOutcomes).AsCollection);
    }

    [Test]
    public async Task GivenProvidedNextCommand_WhenMoveToNext_ReEntersRunningThenFinishes()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");

        await RespondToAskWithNextCommands(nextCommand);

        _sender
            .Setup(mediator => mediator.Send(nextCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.MovePastAsk,
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.ReachedFinalOutcome,
            ClIWorkflowRunStateStatus.Finished,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    // Documents today's selection rule: MoveToNext takes the *last* NextCliCommandOutcome, so a handler
    // that chains on twice has the first silently dropped. Changing that rule is #152.
    [Test]
    public async Task GivenTwoProvidedNextCommands_WhenMoveToNext_SendsOnlyTheLastProvided()
    {
        // Arrange
        var firstProvidedCommand = new TestChainedToCliCommand("first provided");
        var lastProvidedCommand = new TestChainedToCliCommand("last provided");

        await RespondToAskWithNextCommands(firstProvidedCommand, lastProvidedCommand);

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<TestChainedToCliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        _sender.Verify(
            mediator => mediator.Send(lastProvidedCommand, It.IsAny<CancellationToken>()),
            Times.Once);

        _sender.Verify(
            mediator => mediator.Send(firstProvidedCommand, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GivenNoNextCommand_WhenMoveToNext_ReturnsNothingOutcome()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNoNextCommand();

        // Act
        var resultingOutcomes = await _classUnderTest.MoveToNext();

        // Assert
        Assert.That(resultingOutcomes.FirstOrDefault(), Is.InstanceOf<NothingOutcome>());
    }

    [Test]
    public async Task GivenNoNextCommand_WhenMoveToNext_NeverSendsACommand()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNoNextCommand();

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        _sender.Verify(
            mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GivenNoNextCommand_WhenMoveToNext_ChangesStateToInvalidMovePastAskThenFinished()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNoNextCommand();

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.MovePastAsk,
            ClIWorkflowRunStateStatus.InvalidMovePastAsk,
            ClIWorkflowRunStateStatus.Finished,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenSpecifiedNextCommand_WhenRespondToAsk_StateChangesToMovePastAsk()
    {
        // Act
        await RespondToAskWithNextOutcomes(new SpecifiedNextCliCommandOutcome(typeof(TestFactoryBuiltCliCommand)));

        // Assert - the run's guard finds a hop of either kind, not just one carrying an instance.
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.MovePastAsk,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
    }

    [Test]
    public async Task GivenSpecifiedNextCommand_WhenMoveToNext_SendsTheCommandTheFactoryBuilt()
    {
        // Arrange
        var factoryBuiltCommand = new TestFactoryBuiltCliCommand();

        await RespondToAskWithNextOutcomes(new SpecifiedNextCliCommandOutcome(typeof(TestFactoryBuiltCliCommand)));

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(
                It.Is<Instruction>(instruction => instruction.Name == "test-factory-built"),
                It.IsAny<List<Outcome>>()))
            .Returns(factoryBuiltCommand);

        _sender
            .Setup(mediator => mediator.Send(factoryBuiltCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert - the command executed is the factory's, not one the previous handler built.
        _sender.Verify(
            mediator => mediator.Send(factoryBuiltCommand, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GivenSpecifiedNextCommand_WhenMoveToNext_ResolvesAFreshInstructionNamingThatTypeWithPriorOutcomes()
    {
        // Arrange
        await RespondToAskWithNextOutcomes(new SpecifiedNextCliCommandOutcome(typeof(TestFactoryBuiltCliCommand)));

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new TestFactoryBuiltCliCommand());

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<TestFactoryBuiltCliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // A fresh instruction, not the originating ask: the chained command's factory reads the run's
        // artefacts, not arguments the user typed at a different command.
        var expectedInstruction = Instruction.Empty with
        {
            Prefix = "/",
            Name = "test-factory-built"
        };

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert - this is the point of the feature: the factory sees what the run has gathered.
        _cliWorkflowCommandProvider.Verify(
            provider => provider.GetCommand(
                expectedInstruction,
                It.Is<List<Outcome>>(outcomes => outcomes.OfType<RanCliCommandOutcome>().Any())),
            Times.Once);
    }

    [Test]
    public async Task GivenNonDefaultPrefix_WhenMoveToNextToASpecifiedCommand_ThenTheInstructionUsesThatPrefix()
    {
        // Arrange
        var run = new CliWorkflowRun(
            _cliWorkflowRunState,
            _scope.Object,
            _cliInstructionParser.Object,
            _cliInstructionValidator.Object,
            _cliWorkflowCommandProvider.Object,
            Options.Create(new InstructionSettings { Prefix = '!' }),
            _sender.Object,
            _publisher.Object);

        await RespondToAskWithNextOutcomes(
            new SpecifiedNextCliCommandOutcome(typeof(TestFactoryBuiltCliCommand)));

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(new TestFactoryBuiltCliCommand());

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<TestFactoryBuiltCliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await run.MoveToNext();

        // Assert - the prefix is the app's configured one, not a hardcoded slash.
        _cliWorkflowCommandProvider.Verify(
            provider => provider.GetCommand(
                It.Is<Instruction>(instruction => instruction.Prefix == "!"),
                It.IsAny<List<Outcome>>()),
            Times.Once);
    }

    [Test]
    public async Task GivenSpecifiedNextCommandTheFactoryCannotBuild_WhenMoveToNext_RunBecomesExceptional()
    {
        // Arrange
        await RespondToAskWithNextOutcomes(new SpecifiedNextCliCommandOutcome(typeof(TestFactoryBuiltCliCommand)));

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Throws<NoCommandGeneratorException>();

        // Act
        var resultingOutcomes = await _classUnderTest.MoveToNext();

        // Assert - an unbuildable hop is an engineering error, so it fails loudly rather than suggesting.
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.MovePastAsk,
            ClIWorkflowRunStateStatus.Running,
            ClIWorkflowRunStateStatus.Exceptional,
            ClIWorkflowRunStateStatus.Finished,
        };

        var stateChangeTypes = _cliWorkflowRunState
            .Changes
            .Select(x => x.To);

        Assert.Multiple(() =>
        {
            Assert.That(expectedStateChangeTypes, Is.EqualTo(stateChangeTypes).AsCollection);
            Assert.That(resultingOutcomes.FirstOrDefault(), Is.InstanceOf<ExceptionOutcome>());
        });
    }

    /// <summary>
    /// Drives the run through one ask whose handler provides the given commands, leaving it at
    /// <see cref="ClIWorkflowRunStateStatus.MovePastAsk"/> with those commands waiting.
    /// </summary>
    private Task RespondToAskWithNextCommands(params CliCommand[] nextCommands)
        => RespondToAskWithNextOutcomes(nextCommands
            .Select(NextCliCommandOutcome (nextCommand) => new ProvidedNextCliCommandOutcome(nextCommand))
            .ToArray());

    /// <summary>
    /// Drives the run through one ask whose handler returns the given next-command outcomes, leaving it
    /// at <see cref="ClIWorkflowRunStateStatus.MovePastAsk"/>. Works for either kind.
    /// </summary>
    private async Task RespondToAskWithNextOutcomes(params NextCliCommandOutcome[] nextOutcomes)
    {
        var firstCommand = new TestChainingCliCommand();

        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(OriginatingInstruction);

        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(firstCommand);

        _sender
            .Setup(mediator => mediator.Send(firstCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextOutcomes.ToArray<Outcome>());

        _ = await _classUnderTest.RespondToAsk("some valid ask");
    }

    /// <summary>
    /// Puts the run at <see cref="ClIWorkflowRunStateStatus.MovePastAsk"/> with no next command, which
    /// a run never reaches on its own - the guard only fires for a caller invoking
    /// <see cref="CliWorkflowRun.MoveToNext"/> out of step with the run's real history.
    /// </summary>
    private void ArrangeRunAtMovePastAskWithNoNextCommand()
    {
        _cliWorkflowRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        _cliWorkflowRunState.ChangeTo(ClIWorkflowRunStateStatus.MovePastAsk);
    }
}
