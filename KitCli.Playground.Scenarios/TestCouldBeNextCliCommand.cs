using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Declared could-be-next commands. This command ends the run immediately — it never parks — so the
/// moves declared here print right away instead of waiting on a failed follow-up ask. Type
/// <c>/test-could-be-next</c>.
/// </summary>
[CliNextCommandCouldBe("test-could-be-follow-up", "Pick up where /test-could-be-next left off.")]
[CliNextCommandCouldBe("tcbfu", "The short name for /test-could-be-follow-up.")]
public record TestCouldBeNextCliCommand : CliCommand;

public class TestCouldBeNextCliCommandHandler : CliCommandHandler<TestCouldBeNextCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestCouldBeNextCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallyDoingNothing()
            .EndAsync();
}

public record TestCouldBeFollowUpCliCommand : CliCommand;

public class TestCouldBeFollowUpCliCommandHandler : CliCommandHandler<TestCouldBeFollowUpCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestCouldBeFollowUpCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("Reached via a could-be-next suggestion!")
            .EndAsync();
}
