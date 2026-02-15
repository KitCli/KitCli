using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Tests.TestCli.Commands;

public record TestNothingCliCommand : CliCommand;

public class TestNothingCLiCommandHandler : CliCommandHandler<TestNothingCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestNothingCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallyDoingNothing()
            .EndAsync();
}