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
}