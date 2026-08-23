using KitCli.Commands.Abstractions;
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
using Moq;
using NUnit.Framework;

namespace KitCli.Workflow.Tests;

/// <summary>
/// Drives a real <see cref="CliWorkflowRun"/> through each of its four terminal paths
/// (<see cref="ClIWorkflowRunStateStatus.ReachedFinalOutcome"/>, <see cref="ClIWorkflowRunStateStatus.InvalidAsk"/>,
/// <see cref="ClIWorkflowRunStateStatus.Exceptional"/>, <see cref="ClIWorkflowRunStateStatus.InvalidMovePastAsk"/>)
/// and asserts the invariant that neither the rules-engine tests nor the per-scenario
/// <see cref="CliWorkflowRunTests"/> checks: every path must actually reach
/// <see cref="ClIWorkflowRunStateStatus.Finished"/>, and a subsequently-finished run must be retired by
/// <see cref="CliWorkflow.NextRun"/> rather than handed back.
/// </summary>
[TestFixture]
public class CliWorkflowRunTerminalPathInvariantTests
{
    private static async Task<ICliWorkflowRun> ViaReachedFinalOutcome()
    {
        var state = new CliWorkflowRunState();
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var parser = new Mock<IInstructionParser>();
        parser.Setup(p => p.Parse(It.IsAny<string>())).Returns(instruction);

        var validator = new Mock<IInstructionValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<Instruction>())).Returns(true);

        var provider = new Mock<ICliWorkflowCommandProvider>();
        provider.Setup(p => p.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>())).Returns(new CliCommand());

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync([new NothingOutcome()]);

        var run = new CliWorkflowRun(
            state,
            new Mock<IServiceScope>().Object,
            parser.Object,
            validator.Object,
            provider.Object,
            sender.Object,
            new Mock<IPublisher>().Object);

        await run.RespondToAsk("some valid ask");

        return run;
    }

    private static async Task<ICliWorkflowRun> ViaInvalidAsk()
    {
        var state = new CliWorkflowRunState();

        var run = new CliWorkflowRun(
            state,
            new Mock<IServiceScope>().Object,
            new Mock<IInstructionParser>().Object,
            new Mock<IInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<ISender>().Object,
            new Mock<IPublisher>().Object);

        await run.RespondToAsk(string.Empty);

        return run;
    }

    private static async Task<ICliWorkflowRun> ViaExceptional()
    {
        var state = new CliWorkflowRunState();
        var instruction = new Instruction("/", "some-valid-ask", null, []);

        var parser = new Mock<IInstructionParser>();
        parser.Setup(p => p.Parse(It.IsAny<string>())).Returns(instruction);

        var validator = new Mock<IInstructionValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<Instruction>())).Returns(true);

        var provider = new Mock<ICliWorkflowCommandProvider>();
        provider.Setup(p => p.GetCommand(It.IsAny<Instruction>(), It.IsAny<List<Outcome>>())).Returns(new CliCommand());

        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<CliCommand>(), It.IsAny<CancellationToken>())).Throws<Exception>();

        var run = new CliWorkflowRun(
            state,
            new Mock<IServiceScope>().Object,
            parser.Object,
            validator.Object,
            provider.Object,
            sender.Object,
            new Mock<IPublisher>().Object);

        await run.RespondToAsk("/some-valid-ask");

        return run;
    }

    private static async Task<ICliWorkflowRun> ViaInvalidMovePastAsk()
    {
        // MoveToNext() is only ever called by a host after WasChangedTo(MovePastAsk), which in
        // practice always carries a NextCliCommandOutcome with it - so this state is built by hand
        // rather than driven through RespondToAsk, the same way the defensive guard itself can only
        // be reached by a caller invoking MoveToNext() out of step with the run's real history.
        var state = new CliWorkflowRunState();
        state.ChangeTo(ClIWorkflowRunStateStatus.Running);
        state.ChangeTo(ClIWorkflowRunStateStatus.MovePastAsk);

        var run = new CliWorkflowRun(
            state,
            new Mock<IServiceScope>().Object,
            new Mock<IInstructionParser>().Object,
            new Mock<IInstructionValidator>().Object,
            new Mock<ICliWorkflowCommandProvider>().Object,
            new Mock<ISender>().Object,
            new Mock<IPublisher>().Object);

        await run.MoveToNext();

        return run;
    }

    private static IEnumerable<TestCaseData> TerminalPathBuilders()
    {
        yield return new TestCaseData((Func<Task<ICliWorkflowRun>>)ViaReachedFinalOutcome)
            .SetName("GivenRunReachesFinalOutcome_ThenRunFinishesAndIsRetired");

        yield return new TestCaseData((Func<Task<ICliWorkflowRun>>)ViaInvalidAsk)
            .SetName("GivenRunReachesInvalidAsk_ThenRunFinishesAndIsRetired");

        yield return new TestCaseData((Func<Task<ICliWorkflowRun>>)ViaExceptional)
            .SetName("GivenRunReachesExceptional_ThenRunFinishesAndIsRetired");

        yield return new TestCaseData((Func<Task<ICliWorkflowRun>>)ViaInvalidMovePastAsk)
            .SetName("GivenRunReachesInvalidMovePastAsk_ThenRunFinishesAndIsRetired");
    }

    [TestCaseSource(nameof(TerminalPathBuilders))]
    public async Task GivenRunEndsViaTerminalPath_ThenRunReachesFinishedAndIsRetiredByNextRun(
        Func<Task<ICliWorkflowRun>> buildRun)
    {
        // Arrange
        var run = await buildRun();
        var workflow = BuildWorkflow();
        workflow.Runs.Add(run);

        // Assert - every terminal path must actually reach Finished, not just a status one step short of it.
        Assert.That(run.State.WasChangedTo(ClIWorkflowRunStateStatus.Finished), Is.True);

        // Act
        var nextRun = workflow.NextRun();

        // Assert - a Finished run must be retired, never handed back for another ask.
        Assert.That(nextRun, Is.Not.SameAs(run));
    }

    private static CliWorkflow BuildWorkflow()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock.Setup(sp => sp.GetService(typeof(IInstructionParser)))
            .Returns(new Mock<IInstructionParser>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IInstructionValidator)))
            .Returns(new Mock<IInstructionValidator>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ICliWorkflowCommandProvider)))
            .Returns(new Mock<ICliWorkflowCommandProvider>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ISender)))
            .Returns(new Mock<ISender>().Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IPublisher)))
            .Returns(new Mock<IPublisher>().Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.SetupGet(scope => scope.ServiceProvider).Returns(serviceProviderMock.Object);

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        serviceScopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(serviceScopeMock.Object);

        return new CliWorkflow(serviceScopeFactoryMock.Object);
    }
}
