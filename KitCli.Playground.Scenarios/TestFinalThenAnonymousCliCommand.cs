using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// An anonymous outcome after a final one. The anonymous outcome has no say, so the run ends. Type
/// <c>/test-final-then-anonymous</c>.
/// </summary>
public record TestFinalThenAnonymousCliCommand : CliCommand;

public class TestFinalThenAnonymousCliCommandHandler : CliCommandHandler<TestFinalThenAnonymousCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestFinalThenAnonymousCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByFinallySaying("This ends the run.")
            .BySaying("This is said after, and the run still ends.")
            .EndAsync();
}

/// <summary>
/// An anonymous outcome after a reusable one. The anonymous outcome has no say, so the run parks. Type
/// <c>/test-reusable-then-anonymous</c>.
/// </summary>
public record TestReusableThenAnonymousCliCommand : CliCommand;

public class TestReusableThenAnonymousCliCommandHandler : CliCommandHandler<TestReusableThenAnonymousCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestReusableThenAnonymousCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .ByRememberingPageSize(10)
            .BySaying("Page size remembered, and the run parks.")
            .EndAsync();
}
