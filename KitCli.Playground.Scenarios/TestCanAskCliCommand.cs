using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

public record TestCanAskCliCommand : CliCommand;

public class TestCanAskCliCommandHandler(ICliIo io) : CliCommandHandler<TestCanAskCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestCanAskCliCommand command, CancellationToken cancellationToken)
    {
        var whoIsAttached = io.CanAsk
            ? "Someone is attached to the input: the next ask comes from the console."
            : "Nobody is attached to the input: this app is headless.";

        return FinishThisCommand()
            .ByFinallySaying(whoIsAttached)
            .EndAsync();
    }
}
