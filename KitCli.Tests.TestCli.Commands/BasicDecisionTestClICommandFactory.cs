using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Tests.TestCli.Commands;

public record BasicDecisionTestCliCommand : CliCommand;

public class BasicDecisionTestClICommandFactory : BasicDecisionCliCommandFactory<BasicDecisionTestCliCommand>
{
    public override bool CanCreateWhen() => SubCommandIs("test");
}

public class BasicDecisionTestClICommandHandler : CliCommandHandler<BasicDecisionTestCliCommand>
{
    public override Task<Outcome[]> HandleCommand(BasicDecisionTestCliCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Outcome[]
        {
            new FinalMessageOutcome("Basic Decision Test Command Ran")
        });
    }
}