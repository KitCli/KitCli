using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
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
using Moq;
using NUnit.Framework;

namespace KitCli.Workflow.Tests.Run;

[TestFixture]
public class CliWorkflowRunTests
{
    private abstract record TestAggregate;

    private class TestListAggregator : CliListAggregator<TestAggregate>
    {
        protected override IEnumerable<TestAggregate> ListAggregate() => new List<TestAggregate>();
    }
    
    private CliWorkflowRunState _cliWorkflowRunState;
    private Mock<ICliInstructionParser> _cliInstructionParser;
    private Mock<ICliInstructionValidator> _cliInstructionValidator;
    private Mock<ICliWorkflowCommandProvider> _cliWorkflowCommandProvider;
    private Mock<ISender> _sender;
    private Mock<IPublisher> _publisher;
    private CliWorkflowRun _classUnderTest;
    
    [SetUp]
    public void SetUp()
    {
        // Arrange
        _cliWorkflowRunState = new CliWorkflowRunState();
        _cliInstructionParser = new Mock<ICliInstructionParser>();
        _cliInstructionValidator = new Mock<ICliInstructionValidator>();
        _cliWorkflowCommandProvider = new Mock<ICliWorkflowCommandProvider>();
        _sender = new Mock<ISender>();
        _publisher = new Mock<IPublisher>();
        
        _classUnderTest = new CliWorkflowRun(
            _cliWorkflowRunState,
            _cliInstructionParser.Object,
            _cliInstructionValidator.Object,
            _cliWorkflowCommandProvider.Object,
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
        
        Assert.That(firstOutcome, Is.InstanceOf<NothingCliCommandOutcome>());
    }
    
    [Test]
    public async Task GivenInvalidAsk_WhenRespondToAsk_ChangesStateToInvalidAsk()
    {
        // Arrange
        var ask = string.Empty;
        
        // Act
        _ = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var lastStateChange = _cliWorkflowRunState
            .Changes
            .LastOrDefault();
        
        Assert.That(lastStateChange, Is.Not.Null);
        Assert.That(lastStateChange.To, Is.EqualTo(ClIWorkflowRunStateStatus.InvalidAsk));
    }
    
    [Test]
    public async Task GivenInstructionParserFails_WhenRespondToAsk_StateChangeBeforeFinishIsInvalidAsk()
    {
        // Arrange
        var ask = "some valid ask";

        _cliInstructionValidator
            .Setup(civ => civ.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(false);
        
        // Act
        _ = await _classUnderTest.RespondToAsk(ask);
        
        // Assert
        var expectedStateChangeTypes = new[]
        {
            ClIWorkflowRunStateStatus.InvalidAsk,
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
            .Returns(new CliInstruction("prefix", "name", null, []));
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<CliInstruction>(), It.IsAny<List<CliCommandOutcome>>()))
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
        
        var instruction = new CliInstruction("/", "some-valid-ask", null, []);
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<CliInstruction>(), It.IsAny<List<CliCommandOutcome>>()))
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
        
        var instruction = new CliInstruction("/", "some-valid-ask", null, []);

        var nothingOutcome = new NothingCliCommandOutcome();
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<CliInstruction>(), It.IsAny<List<CliCommandOutcome>>()))
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
        
        var instruction = new CliInstruction("/", "some-valid-ask", null, []);

        var aggregator = new TestListAggregator();
        var outcome = new CliCommandAggregatorOutcome<IEnumerable<TestAggregate>>(aggregator);
        
        _cliInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);
        
        _cliInstructionValidator
            .Setup(civ => civ.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(true);
        
        _cliWorkflowCommandProvider
            .Setup(provider => provider.GetCommand(It.IsAny<CliInstruction>(), It.IsAny<List<CliCommandOutcome>>()))
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
}