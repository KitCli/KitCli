using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Says back the sub-command name exactly as it was parsed. Typing
/// <c>/test-sub-command-path user edit cir</c> shows every word after the command name arriving as one
/// string, rather than as a path of separate words a command could match against. Recorded in
/// <c>docs/investigations/0009-how-many-sub-commands-can-one-instruction-address.md</c>.
/// </summary>
/// <param name="SubCommandName">The sub-command name the factory read off the instruction.</param>
public record TestSubCommandPathCliCommand(string SubCommandName) : CliCommand;

public class TestSubCommandPathCliCommandFactory : BasicCreationCliCommandFactory<TestSubCommandPathCliCommand>
{
    public override CliCommand Create()
        => new TestSubCommandPathCliCommand(Instruction.SubInstructionName ?? string.Empty);
}

public class TestSubCommandPathCliCommandHandler : CliCommandHandler<TestSubCommandPathCliCommand>
{
    public override Task<Outcome[]> HandleCommand(
        TestSubCommandPathCliCommand command,
        CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying($"Sub-command name: \"{command.SubCommandName}\"")
            .EndAsync();
}
