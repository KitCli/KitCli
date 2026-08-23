using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record TestAliasedCliCommand : CliCommand;

public class TestAliasedCliCommandHandler : CliCommandHandler<TestAliasedCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestAliasedCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("Reached via an alias!")
            .EndAsync();
}
