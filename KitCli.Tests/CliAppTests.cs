using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Io;
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

namespace KitCli.Tests;

[TestFixture]
public class CliAppTests
{
    private static readonly IOptions<InstructionSettings> DefaultInstructionSettings =
        Options.Create(new InstructionSettings());

    private CliWorkflowRunState _workflowRunState;
    private Mock<IServiceScope> _mockScope;
    private Mock<IInstructionParser> _mockInstructionParser;
    private Mock<IInstructionValidator> _mockInstructionValidator;
    private Mock<ICliWorkflowCommandProvider> _mockWorkflowCommandProvider;
    private Mock<ICliWorkflowReactionProvider> _mockWorkflowReactionProvider;
    private Mock<ISender> _mockSender;
    private Mock<IPublisher> _mockPublisher;
    private CliWorkflowRun _workflowRun;
    
    private Mock<ICliWorkflow> _mockCliWorkflow;
    private Mock<IEnumerable<IOutcomeIoWriter>> _mockOutcomeIoWriters;
    private Mock<ICliIo> _mockCliIo;
    private TestCliApp _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        SetUpWorkflowRun();
        
        _mockCliWorkflow = new Mock<ICliWorkflow>();
        _mockOutcomeIoWriters = new Mock<IEnumerable<IOutcomeIoWriter>>();
        _mockOutcomeIoWriters
            .Setup(w => w.GetEnumerator())
            .Returns(new List<IOutcomeIoWriter>().GetEnumerator());
        _mockCliIo = new Mock<ICliIo>();
        _classUnderTest = new TestCliApp(
            _mockCliWorkflow.Object,
            _mockCliIo.Object);
    }

    private void SetUpWorkflowRun()
    {
        _workflowRunState = new CliWorkflowRunState();
        _mockScope = new Mock<IServiceScope>();
        _mockInstructionParser = new Mock<IInstructionParser>();
        _mockInstructionValidator = new Mock<IInstructionValidator>();
        _mockWorkflowCommandProvider = new Mock<ICliWorkflowCommandProvider>();
        _mockWorkflowReactionProvider = new Mock<ICliWorkflowReactionProvider>();
        _mockSender = new Mock<ISender>();
        _mockPublisher = new Mock<IPublisher>();
        
        _workflowRun = new CliWorkflowRun(
            _workflowRunState,
            _mockScope.Object,
            _mockInstructionParser.Object,
            _mockInstructionValidator.Object,
            _mockWorkflowCommandProvider.Object,
            _mockWorkflowReactionProvider.Object,
            DefaultInstructionSettings,
            _mockSender.Object,
            _mockPublisher.Object);
    }

    [Test]
    public async Task GivenCliApp_WhenRun_CreatesNewRun()
    {
        // Arrange
        _mockCliWorkflow
            .Setup(w => w.NextRun())
            .Returns(_workflowRun);

        _mockCliIo
            .Setup(io => io.AskAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("/some-valid-ask");
        
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        _mockInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);

        _mockInstructionValidator
            .Setup(v => v.IsValid(It.IsAny<Instruction>()))
            .Returns(() =>
            {
                _mockCliWorkflow
                    .Setup(w => w.Status)
                    .Returns(CliWorkflowStatus.Stopped);
                
                return false;
            });
        
        // Act
        await _classUnderTest.Run(_mockOutcomeIoWriters.Object.ToList()); // Starts a while loop, awaiting lets it run once.
        
        // Assertx
        _mockCliWorkflow.Verify(w => w.NextRun(), Times.Once);
    }

    [Test]
    public async Task GivenChainOfMovePastAskSteps_WhenExecuteRunOperation_ThenPausesOnlyOnce()
    {
        // Arrange
        var secondCommand = new CliCommand();
        var thirdCommand = new CliCommand();

        var instruction = new Instruction("/", "chain", null, []);

        _mockCliWorkflow
            .Setup(w => w.NextRun())
            .Returns(_workflowRun);

        _mockInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);

        _mockInstructionValidator
            .Setup(v => v.IsValid(It.IsAny<Instruction>()))
            .Returns(true);

        _mockWorkflowCommandProvider
            .Setup(p => p.GetCommand(instruction, It.IsAny<List<Outcome>>()))
            .Returns(new CliCommand());

        _mockSender
            .SetupSequence(s => s.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SayOutcome("Link 1"), new ProvidedNextCliCommandOutcome(secondCommand)])
            .ReturnsAsync([new SayOutcome("Link 2"), new ProvidedNextCliCommandOutcome(thirdCommand)])
            .ReturnsAsync([new FinalSayOutcome("Chain complete")]);

        // Act
        await _classUnderTest.ExecuteRunOperationForTests("chain", _mockOutcomeIoWriters.Object.ToList());

        // Assert
        _mockCliIo.Verify(io => io.Pause(), Times.Once);
    }

    private class TestCliApp(ICliWorkflow workflow, ICliIo io) : CliApp(workflow, io)
    {
        public Task<Outcome[]> ExecuteRunOperationForTests(string ask, List<IOutcomeIoWriter> outcomeIoWriters)
            => ExecuteRunOperation(ask, outcomeIoWriters);
    }
}