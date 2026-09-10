using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Playground.Scenarios;

public record TestStackTraceTableCliCommand : CliCommand;

public class TestStackTraceTableCliCommandHandler : CliCommandHandler<TestStackTraceTableCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestStackTraceTableCliCommand command, CancellationToken cancellationToken)
    {
        var table = new Table(
            ["Step", "Outcome"],
            [
                ["(load)", "succeeded"],
                ["(run)", CaughtStackTrace()],
                ["(save)", "succeeded"]
            ]);

        return FinishThisCommand()
            .ByShowingTable(table)
            .EndAsync();
    }

    private static string CaughtStackTrace()
    {
        try
        {
            ThrowSoTheStackTraceIsReal();
            return string.Empty;
        }
        catch (InvalidOperationException exception)
        {
            return exception.ToString();
        }
    }

    private static void ThrowSoTheStackTraceIsReal()
        => throw new InvalidOperationException("The connector could not be reached.");
}
