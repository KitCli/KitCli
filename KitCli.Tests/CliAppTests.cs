using KitCli.Commands.Abstractions.Io.Outcomes;
using KitCli.Commands.Abstractions.Outcomes;
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
    private Mock<ICliInstructionParser> _mockInstructionParser;
    private Mock<ICliInstructionValidator> _mockInstructionValidator;
    private Mock<ICliWorkflowCommandProvider> _mockWorkflowCommandProvider;
    private Mock<IMediator> _mockMediator;
    private CliWorkflowRun _workflowRun;
    
    private Mock<ICliWorkflow> _mockCliWorkflow;
    private Mock<ICliCommandOutcomeIo> _mockCliIo;
    private TestCliApp _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        SetUpWorkflowRun();
        
        _mockCliWorkflow = new Mock<ICliWorkflow>();
        _mockCliIo = new Mock<ICliCommandOutcomeIo>();
        _classUnderTest = new TestCliApp(_mockCliWorkflow.Object, _mockCliIo.Object);
    }

    private void SetUpWorkflowRun()
    {
        _workflowRunState = new CliWorkflowRunState();
        _mockInstructionParser = new Mock<ICliInstructionParser>();
        _mockInstructionValidator = new Mock<ICliInstructionValidator>();
        _mockWorkflowCommandProvider = new Mock<ICliWorkflowCommandProvider>();
        _mockMediator = new Mock<IMediator>();
        
        _workflowRun = new CliWorkflowRun(
            _workflowRunState,
            _mockInstructionParser.Object,
            _mockInstructionValidator.Object,
            _mockWorkflowCommandProvider.Object,
            _mockMediator.Object);
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
        
        var instruction = new CliInstruction("/", "some-valid-ask", null, []);

        _mockInstructionParser
            .Setup(parser => parser.Parse(It.IsAny<string>()))
            .Returns(instruction);

        _mockInstructionValidator
            .Setup(v => v.IsValidInstruction(It.IsAny<CliInstruction>()))
            .Returns(() =>
            {
                _mockCliWorkflow
                    .Setup(w => w.Status)
                    .Returns(CliWorkflowStatus.Stopped);
                
                return false;
            });
        
        // Act
        await _classUnderTest.Run(); // Starts a while loop, awaiting lets it run once.
        
        // Assert
        _mockCliWorkflow.Verify(w => w.NextRun(), Times.Once);
        _mockCliIo.Verify(io => io.Say(It.IsAny<CliCommandOutcome[]>()), Times.Once);
    }

    private class TestCliApp(ICliWorkflow workflow, ICliCommandOutcomeIo io) : CliApp(workflow, io);
}