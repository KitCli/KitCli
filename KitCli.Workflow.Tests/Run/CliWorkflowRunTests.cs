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
        await RespondToAskQueueing(nextCommand);

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
    public async Task GivenQueuedNextCommand_WhenMoveToNext_SendsThatCommand()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");

        await RespondToAskQueueing(nextCommand);

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
    public async Task GivenQueuedNextCommand_WhenMoveToNext_ReturnsRanCommandOutcomeThenItsOutcomes()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");
        var nothingOutcome = new NothingOutcome();

        await RespondToAskQueueing(nextCommand);

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
    public async Task GivenQueuedNextCommand_WhenMoveToNext_ReEntersRunningThenFinishes()
    {
        // Arrange
        var nextCommand = new TestChainedToCliCommand("next");

        await RespondToAskQueueing(nextCommand);

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

    // Documents today's selection rule: MoveToNext takes the *last* queued NextCliCommandOutcome,
    // so a handler that queues two hops has the first silently dropped. Changing that rule is #152.
    [Test]
    public async Task GivenTwoQueuedNextCommands_WhenMoveToNext_SendsOnlyTheLastQueued()
    {
        // Arrange
        var firstQueuedCommand = new TestChainedToCliCommand("first queued");
        var lastQueuedCommand = new TestChainedToCliCommand("last queued");

        await RespondToAskQueueing(firstQueuedCommand, lastQueuedCommand);

        _sender
            .Setup(mediator => mediator.Send(It.IsAny<TestChainedToCliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new NothingOutcome()]);

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        _sender.Verify(
            mediator => mediator.Send(lastQueuedCommand, It.IsAny<CancellationToken>()),
            Times.Once);

        _sender.Verify(
            mediator => mediator.Send(firstQueuedCommand, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GivenNothingQueued_WhenMoveToNext_ReturnsNothingOutcome()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNothingQueued();

        // Act
        var resultingOutcomes = await _classUnderTest.MoveToNext();

        // Assert
        Assert.That(resultingOutcomes.FirstOrDefault(), Is.InstanceOf<NothingOutcome>());
    }

    [Test]
    public async Task GivenNothingQueued_WhenMoveToNext_NeverSendsACommand()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNothingQueued();

        // Act
        _ = await _classUnderTest.MoveToNext();

        // Assert
        _sender.Verify(
            mediator => mediator.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GivenNothingQueued_WhenMoveToNext_ChangesStateToInvalidMovePastAskThenFinished()
    {
        // Arrange
        ArrangeRunAtMovePastAskWithNothingQueued();

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

    /// <summary>
    /// Drives the run through one ask whose handler queues the given commands, leaving it at
    /// <see cref="ClIWorkflowRunStateStatus.MovePastAsk"/> with those commands waiting.
    /// </summary>
    private async Task RespondToAskQueueing(params CliCommand[] nextCommands)
    {
        var firstCommand = new TestChainingCliCommand();

        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(new Instruction("/", "some-valid-ask", null, []));

        _cliInstructionValidator
            .Setup(civ => civ.IsValid(It.IsAny<Instruction>()))
            .Returns(true);

        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>()))
            .Returns(firstCommand);

        _sender
            .Setup(mediator => mediator.Send(firstCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextCommands
                .Select(Outcome (nextCommand) => new NextCliCommandOutcome(nextCommand))
                .ToArray());

        _ = await _classUnderTest.RespondToAsk("some valid ask");
    }

    /// <summary>
    /// Puts the run at <see cref="ClIWorkflowRunStateStatus.MovePastAsk"/> with no queued command, which
    /// a run never reaches on its own - the guard only fires for a caller invoking
    /// <see cref="CliWorkflowRun.MoveToNext"/> out of step with the run's real history.
    /// </summary>
    private void ArrangeRunAtMovePastAskWithNothingQueued()
    {
        _cliWorkflowRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        _cliWorkflowRunState.ChangeTo(ClIWorkflowRunStateStatus.MovePastAsk);
    }
}
