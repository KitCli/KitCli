using System.Reflection;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Workflow.IntegrationTests.Commands;

/// <summary>
/// Reproduces the scenario reported against #289 and worked through in investigation 0011: a dedicated
/// factory exists for one sub-command only ("there"), and both the plain command and an unrelated,
/// completely uncovered sub-command ("mate") still have to resolve to the right type.
/// </summary>
[TestFixture]
public class CliWorkflowCommandProviderSharedKeyFallbackTests
{
    public record HelloCliCommand : CliCommand;
    public record HelloThereCliCommand : CliCommand;
    public record HelloMateCliCommand : CliCommand;

    public class HelloThereCliCommandFactory : CliCommandFactory<HelloCliCommand>
    {
        public override bool CanCreateWhen() => SubCommandIs("there");
        public override CliCommand Create() => new HelloThereCliCommand();
    }

    private static CliCommand Resolve(string? subInstructionName)
    {
        var services = new ServiceCollection();
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();
        var commandProvider = new CliWorkflowCommandProvider(provider);

        var instruction = new Instruction("/", "hello", subInstructionName, []);
        return commandProvider.GetCommand(instruction, new List<Outcome>());
    }

    [Test]
    public void GivenNoSubCommand_WhenGetCommand_ThenBuildsThePlainCommand()
        => Assert.That(Resolve(null), Is.TypeOf<HelloCliCommand>());

    [Test]
    public void GivenTheSubCommandWithADedicatedFactory_WhenGetCommand_ThenTheDedicatedFactoryBuildsIt()
        => Assert.That(Resolve("there"), Is.TypeOf<HelloThereCliCommand>());

    [Test]
    public void GivenAnUncoveredSubCommand_WhenGetCommand_ThenBuildsThatSiblingWithNoFactoryWrittenForIt()
        => Assert.That(Resolve("mate"), Is.TypeOf<HelloMateCliCommand>());
}
