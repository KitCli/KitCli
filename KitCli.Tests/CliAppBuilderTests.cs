using KitCli.Abstractions;
using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Extensions;
using KitCli.Workflow;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Tests;

[TestFixture]
public class CliAppBuilderTests
{
    [Test]
    public void GivenOnlyValidRegistrations_WhenRun_BuildsTheServiceProvider()
    {
        // Arrange
        var classUnderTest = new CliAppBuilder()
            .WithRegistry<TestCliAppRegistry>();

        // Act
        // Running an ArgsCliApp with no args reaches a guard that only fires once the provider
        // has been built and validated — so this specific throw proves validation passed.
        var run = () => classUnderTest.Run();

        // Assert
        Assert.That(run, Throws.ArgumentException
            .With.Message.Contains("requires at least one argument"));
    }

    [Test]
    public void GivenASingletonDependingOnAScopedService_WhenRun_ThrowsAtStartup()
    {
        // Arrange
        var classUnderTest = new CliAppBuilder()
            .WithRegistry<TestCliAppRegistry>()
            .WithRegistry<TestScopeCapturingRegistry>();

        // Act
        var run = () => classUnderTest.Run(["/some-ask"]);

        // Assert
        Assert.That(run, Throws.InstanceOf<AggregateException>()
            .With.Message.Contains(nameof(TestScopedDependency)));
    }

    private class TestArgsCliApp(ICliWorkflow workflow, ICliIo io) : ArgsCliApp(workflow, io);

    private record TestBuilderCliCommand : CliCommand;

    private class TestBuilderCliCommandHandler : CliCommandHandler<TestBuilderCliCommand>
    {
        public override Task<Outcome[]> HandleCommand(
            TestBuilderCliCommand command,
            CancellationToken cancellationToken)
            => Task.FromResult<Outcome[]>([]);
    }

    /// <summary>
    /// Mirrors what <c>AddCli&lt;TCliApp&gt;</c> registers, but scans this assembly for commands
    /// rather than the entry assembly — under a test host the entry assembly is <c>testhost</c>,
    /// which has no commands (see issue #118).
    /// </summary>
    private class TestCliAppRegistry : ICliAppRegistry
    {
        public void Register(IServiceCollection services)
        {
            services.AddCliAbstractions();
            services.AddCliInstructions();
            services.AddCommandAbstractions();

            services.AddSingleton<ICliWorkflow, CliWorkflow>();

            services.AddCliWorkflowCommands();
            services.AddCommandsFromAssembly(typeof(TestBuilderCliCommand).Assembly);

            services.AddSingleton<CliApp, TestArgsCliApp>();
        }
    }

    private class TestScopedDependency;

    private class TestScopeCapturingWriter(TestScopedDependency dependency) : IOutcomeIoWriter
    {
        public bool CanWriteFor(Outcome outcome) => false;

        public void Write(Outcome outcome) => _ = dependency;
    }

    private class TestScopeCapturingRegistry : ICliAppRegistry
    {
        public void Register(IServiceCollection services)
        {
            services.AddScoped<TestScopedDependency>();
            services.AddOutcomeIoWriter<TestScopeCapturingWriter>();
        }
    }
}
