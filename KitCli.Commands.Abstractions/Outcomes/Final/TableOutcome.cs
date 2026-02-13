using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class TableOutcome(Table table) : Outcome(OutcomeKind.Final)
{
    public Table Table = table;
}