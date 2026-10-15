using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Built by whichever of two <see cref="CliCommandFactory{TCliCommand}"/> picks it via
/// <c>CanCreateWhen()</c>. Typing <c>/test-multiple-factories a</c> or
/// <c>/test-multiple-factories b</c> shows both factories registered for the one command type,
/// rather than <c>AddCommandsFromAssembly</c> throwing at startup.
/// </summary>
/// <param name="MatchedSubCommandName">The sub-command name the matching factory read off the instruction.</param>
public record TestMultipleFactoriesCliCommand(string MatchedSubCommandName) : CliCommand;

public class TestMultipleFactoriesCliCommandFactoryA : CliCommandFactory<TestMultipleFactoriesCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("a");
    public override CliCommand Create() => new TestMultipleFactoriesCliCommand("a");
}

public class TestMultipleFactoriesCliCommandFactoryB : CliCommandFactory<TestMultipleFactoriesCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("b");
    public override CliCommand Create() => new TestMultipleFactoriesCliCommand("b");
}

public class TestMultipleFactoriesCliCommandHandler : CliCommandHandler<TestMultipleFactoriesCliCommand>
{
    public override Task<Outcome[]> HandleCommand(
        TestMultipleFactoriesCliCommand command,
        CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying($"Built by the factory matching sub-command \"{command.MatchedSubCommandName}\"")
            .EndAsync();
}
