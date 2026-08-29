using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
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

namespace KitCli.Workflow.Tests;

[TestFixture]
public class CliWorkflowTests
{
    private record TestOutcome() : Outcome(OutcomeKind.Reusable);

    private static readonly IOptions<InstructionSettings> DefaultInstructionSettings =
        Options.Create(new InstructionSettings());

    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private CliWorkflow _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock
            .SetupGet(scope => scope.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeFactoryMock
            .Setup(factory => factory.CreateScope())
            .Returns(serviceScopeMock.Object);

        _classUnderTest = new CliWorkflow(_serviceScopeFactoryMock.Object);
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
            .Setup(sp => sp.GetService(typeof(IInstructionParser)))
            .Returns(new Mock<IInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IInstructionValidator)))
            .Returns(new Mock<IInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);

        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowReactionProvider)))
            .Returns(new Mock<ICliWorkflowReactionProvider>().Object);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<InstructionSettings>)))
            .Returns(DefaultInstructionSettings);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISender)))
            .Returns(new Mock<ISender>().Object);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPublisher)))
            .Returns(new Mock<IPublisher>().Object);

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
            .Setup(sp => sp.GetService(typeof(IInstructionParser)))
            .Returns(new Mock<IInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IInstructionValidator)))
            .Returns(new Mock<IInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);

        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowReactionProvider)))
            .Returns(new Mock<ICliWorkflowReactionProvider>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISender)))
            .Returns(new Mock<ISender>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPublisher)))
            .Returns(new Mock<IPublisher>().Object);

        var outcome = new TestOutcome();
        
        var reusableRunState = new CliWorkflowRunState();
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome, [outcome]);
        
        var reusableRun = new CliWorkflowRun(
            reusableRunState,
            new Mock<IServiceScope>().Object,
            new Mock<IInstructionParser>().Object,
            new Mock<IInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<ICliWorkflowReactionProvider>().Object,
            DefaultInstructionSettings,
            new Mock<ISender>().Object,
            new Mock<IPublisher>().Object);
        
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
            .Setup(sp => sp.GetService(typeof(IInstructionParser)))
            .Returns(new Mock<IInstructionParser>().Object);
        
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IInstructionValidator)))
            .Returns(new Mock<IInstructionValidator>().Object);
        
        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);

        _serviceProviderMock
            .Setup(sp =>  sp.GetService(typeof(ICliWorkflowReactionProvider)))
            .Returns(new Mock<ICliWorkflowReactionProvider>().Object);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IOptions<InstructionSettings>)))
            .Returns(DefaultInstructionSettings);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ISender)))
            .Returns(new Mock<ISender>().Object);

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IPublisher)))
            .Returns(new Mock<IPublisher>().Object);

        var outcome = new FinalSayOutcome(string.Empty);
        
        var reusableRunState = new CliWorkflowRunState();
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Running);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, [outcome]);
        reusableRunState.ChangeTo(ClIWorkflowRunStateStatus.Finished);

        var reusableRun = new CliWorkflowRun(
            reusableRunState,
            new Mock<IServiceScope>().Object,
            new Mock<IInstructionParser>().Object,
            new Mock<IInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<ICliWorkflowReactionProvider>().Object,
            DefaultInstructionSettings,
            new Mock<ISender>().Object,
            new Mock<IPublisher>().Object);
        
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

    [Test]
    public void GivenRunning_WhenInterruptCurrentRun_ThenWorkflowStopsRunning()
    {
        // Act
        _classUnderTest.InterruptCurrentRun();

        // Assert
        Assert.That(_classUnderTest.Status, Is.EqualTo(CliWorkflowStatus.Stopped));
    }

    [Test]
    public void GivenRunning_WhenInterruptCurrentRun_ThenCancellationTokenIsCancelled()
    {
        // Act
        _classUnderTest.InterruptCurrentRun();

        // Assert
        Assert.That(_classUnderTest.CancellationToken.IsCancellationRequested, Is.True);
    }

    [Test]
    public void GivenRunning_WhenStop_ThenCancellationTokenIsNotCancelled()
    {
        // Act
        _classUnderTest.Stop();

        // Assert
        Assert.That(_classUnderTest.CancellationToken.IsCancellationRequested, Is.False);
    }
}