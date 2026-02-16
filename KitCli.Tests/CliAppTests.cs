using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
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

namespace KitCli.Tests;

[TestFixture]
public class CliAppTests
{
    private CliWorkflowRunState _workflowRunState;
    private Mock<IInstructionParser> _mockInstructionParser;
    private Mock<IInstructionValidator> _mockInstructionValidator;
    private Mock<ICliWorkflowCommandProvider> _mockWorkflowCommandProvider;
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
        _mockCliIo = new Mock<ICliIo>();
        _classUnderTest = new TestCliApp(
            _mockCliWorkflow.Object,
            _mockCliIo.Object);
    }

    private void SetUpWorkflowRun()
    {
        _workflowRunState = new CliWorkflowRunState();
        _mockInstructionParser = new Mock<IInstructionParser>();
        _mockInstructionValidator = new Mock<IInstructionValidator>();
        _mockWorkflowCommandProvider = new Mock<ICliWorkflowCommandProvider>();
        _mockSender = new Mock<ISender>();
        _mockPublisher = new Mock<IPublisher>();
        
        _workflowRun = new CliWorkflowRun(
            _workflowRunState,
            _mockInstructionParser.Object,
            _mockInstructionValidator.Object,
            _mockWorkflowCommandProvider.Object,
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
            .Setup(io => io.Ask())
            .Returns("/some-valid-ask");
        
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

    private class TestCliApp(ICliWorkflow workflow, ICliIo io) : CliApp(workflow, io);
}