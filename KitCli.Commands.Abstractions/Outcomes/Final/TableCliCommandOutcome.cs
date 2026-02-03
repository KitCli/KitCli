using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class TableCliCommandOutcome(Table table) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public Table Table = table;
}