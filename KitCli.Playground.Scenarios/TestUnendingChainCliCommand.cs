using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// Recursion with no base case. Handing back to your own command is legitimate — a countdown or a
/// retry does it, and ends when some pass returns a final outcome. This handler has no such pass, so
/// the run queues another step every time it takes one. <b>Running this never returns</b>, and it
/// grows the run's history for as long as it spins. It is here to demonstrate that nothing detects
/// the difference. See <see href="https://github.com/KitCli/KitCli/issues/173"/>.
/// </summary>
public record TestUnendingChainCliCommand : CliCommand;

public class TestUnendingChainCliCommandHandler : CliCommandHandler<TestUnendingChainCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestUnendingChainCliCommand command, CancellationToken cancellationToken)
        => FinishThisCommand()
            .BySaying("Round the chain again")
            .ByMovingToCommand(new TestUnendingChainCliCommand())
            .EndAsync();
}
