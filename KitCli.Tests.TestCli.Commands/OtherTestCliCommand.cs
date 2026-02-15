using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Tests.TestCli.Commands;

public record OtherTestCliCommand : CliCommand;

public class OtherTestCliCommandHandler : CliCommandHandler<OtherTestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(OtherTestCliCommand request, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("Other Command Ran")
            .EndAsync();
}