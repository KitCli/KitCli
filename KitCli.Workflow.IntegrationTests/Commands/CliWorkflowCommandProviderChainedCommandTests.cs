using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Workflow.IntegrationTests.Commands;

/// <summary>
/// Covers what chaining to a command by type relies on: that the name
/// <see cref="CliCommand.GetInstructionName(Type)"/> derives from a command type is the key that command's
/// factory is registered under, so an instruction carrying that name resolves it and hands it the run's
/// artefacts. <c>CliWorkflowRun</c> builds exactly this instruction; here it is built by hand against real
/// registration, which a mocked provider cannot prove.
/// </summary>
[TestFixture]
public class CliWorkflowCommandProviderChainedCommandTests
{
    private record ChainedToCliCommand(string SeenArtefactName) : CliCommand;

    private record ChainedToUnbuildableCliCommand(string NeedsAnArgument) : CliCommand;

    private record ChainTestOutcome() : Outcome(OutcomeKind.Reusable);

    private record ChainTestArtefact() : AnonymousArtefact("gathered-by-the-run");

    private class ChainTestArtefactFactory : ArtefactFactory<ChainTestOutcome>
    {
        protected override AnonymousArtefact CreateArtefact(ChainTestOutcome outcome) => new ChainTestArtefact();
    }

    private class ChainedToCliCommandFactory : CliCommandFactory<ChainedToCliCommand>
    {
        public override bool CanCreateWhen() => true;

        public override CliCommand Create()
            => new ChainedToCliCommand(Artefacts.FirstOrDefault()?.Name ?? "no artefacts");
    }

    private ServiceProvider _serviceProvider;
    private CliWorkflowCommandProvider _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        services
            .AddKeyedSingleton<ICliCommandFactory, ChainedToCliCommandFactory>(
                CliCommand.GetInstructionName(typeof(ChainedToCliCommand)))
            .AddSingleton<IArtefactFactory, ChainTestArtefactFactory>();

        _serviceProvider = services.BuildServiceProvider();
        _classUnderTest = new CliWorkflowCommandProvider(_serviceProvider);
    }

    [TearDown]
    public void TearDown() => _serviceProvider.Dispose();

    /// <summary>Mirrors the instruction <c>CliWorkflowRun</c> builds for a specified next command.</summary>
    private static Instruction InstructionNaming(Type commandType)
        => Instruction.Empty with
        {
            Prefix = "/",
            Name = CliCommand.GetInstructionName(commandType)
        };

    [Test]
    public void GivenInstructionNamingAChainedCommandType_WhenGetCommand_ThenFactoryBuildsItFromTheRunsArtefacts()
    {
        // Arrange
        var outcomes = new List<Outcome> { new ChainTestOutcome() };

        // Act
        var command = _classUnderTest.GetCommand(InstructionNaming(typeof(ChainedToCliCommand)), outcomes);

        // Assert - built from a run artefact, not from a previous handler's local variable.
        Assert.That(command, Is.EqualTo(new ChainedToCliCommand("gathered-by-the-run")));
    }

    [Test]
    public void GivenInstructionNamingATypeWithNoFactory_WhenGetCommand_ThenThrowsNoCommandGenerator()
    {
        // Act & Assert - a command with constructor arguments and no dedicated factory is never registered,
        // and no compiler check can catch a chain to it.
        Assert.Throws<NoCommandGeneratorException>(
            () => _classUnderTest.GetCommand(InstructionNaming(typeof(ChainedToUnbuildableCliCommand)), []));
    }
}
