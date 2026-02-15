using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Tests.TestCli.Commands;

public record NoFactoryTestCliCommand : CliCommand;

public class NoFactoryTestCliCommandHandler : CliCommandHandler<NoFactoryTestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NoFactoryTestCliCommand request, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("No Factory Command Ran")
            .EndAsync();
}