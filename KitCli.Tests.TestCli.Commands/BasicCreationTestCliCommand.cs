using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Tests.TestCli.Commands;

public record BasicCreationTestCliCommand(string Test) : CliCommand;

public class BasicCreationTestClICommandFactory : BasicCreationCliCommandFactory<BasicCreationTestCliCommand>
{
    public override CliCommand Create() => new BasicCreationTestCliCommand("Basic Creation Test Command Ran");
}

public class BasicCreationTestClICommandHandler : CliCommandHandler<BasicCreationTestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(BasicCreationTestCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying(command.Test)
            .EndAsync();
}