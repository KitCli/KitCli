using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class CliCommandTableOutcome(CliTable table) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public CliTable Table = table;
}