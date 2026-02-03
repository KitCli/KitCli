using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run;
using KitCli.Workflow.Run.State;
using MediatR;
using Moq;
using NUnit.Framework;

namespace KitCli.Workflow.Tests;

[TestFixture]
public class CliWorkflowTests
{
    private class TestCliCommandOutcome() : CliCommandOutcome(CliCommandOutcomeKind.Reusable);
    
    private Mock<IServiceProvider> _serviceProviderMock;
    private CliWorkflow _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _classUnderTest = new CliWorkflow(_serviceProviderMock.Object);
    }

    [Test]
    public void GivenCreated_WhenConstructor_HasStartedStatus()
    {
        Assert.That(_classUnderTest.Status, Is.EqualTo(CliWorkflowStatus.Started));
    }
    
    [Test]
    public void GivenCreated_WhenNextRun_CreatesNewRun()
    {
        // Arrange
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionParser)))
            .Returns(new Mock<ICliInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionValidator)))
            .Returns(new Mock<ICliInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(new Mock<IMediator>().Object);
        
        // Act
        var run = _classUnderTest.NextRun();
        
        // Assert
        Assert.That(run, Is.Not.Null);
        Assert.That(_classUnderTest.Runs, Has.Member(run));
    }
    
    [Test]
    public void GivenPriorRunAchievedReusableOutcome_WhenNextRun_GetsThatRun()
    {
        // Arrange
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionParser)))
            .Returns(new Mock<ICliInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionValidator)))
            .Returns(new Mock<ICliInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(new Mock<IMediator>().Object);

        var outcome = new TestCliCommandOutcome();
        
        var reusableRunState = new CliWorkflowRunState();
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome, [outcome]);
        
        var reusableRun = new CliWorkflowRun(
            reusableRunState,
            new Mock<ICliInstructionParser>().Object,
            new Mock<ICliInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<IMediator>().Object);
        
        _classUnderTest.Runs.Add(reusableRun);
        
        // Act
        var nextRun = _classUnderTest.NextRun();
        
        // Assert
        Assert.That(nextRun, Is.EqualTo(reusableRun));
    }
    
    [Test]
    public void GivenPriorRunAchievedFinalOutcome_WhenNextRun_GetsThatRun()
    {
        // Arrange
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionParser)))
            .Returns(new Mock<ICliInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ICliInstructionValidator)))
            .Returns(new Mock<ICliInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(new Mock<IMediator>().Object);
        
        var outcome = new OutputCliCommandOutcome(string.Empty);
        
        var reusableRunState = new CliWorkflowRunState();
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [outcome]);
        
        var reusableRun = new CliWorkflowRun(
            reusableRunState,
            new Mock<ICliInstructionParser>().Object,
            new Mock<ICliInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<IMediator>().Object);
        
        _classUnderTest.Runs.Add(reusableRun);
        
        // Act
        var nextRun = _classUnderTest.NextRun();
        
        // Assert
        Assert.That(nextRun, Is.Not.EqualTo(reusableRun));
    }
    
    [Test]
    public void GivenRunning_WhenStop_ThenWorkflowStopsRunning()
    {
        // Act
        _classUnderTest.Stop();
        
        // Assert
        Assert.That(_classUnderTest.Status, Is.EqualTo(CliWorkflowStatus.Stopped));
    }
}