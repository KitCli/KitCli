using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

/// <summary>
/// A chain with no way out: the handler hands straight back to its own command, so the run queues
/// another step every time it takes one and never reaches a final outcome. <b>Running this never
/// returns</b> — it is here to demonstrate that nothing detects it, and it grows the run's history
/// for as long as it spins. See <see href="https://github.com/KitCli/KitCli/issues/173"/>.
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
