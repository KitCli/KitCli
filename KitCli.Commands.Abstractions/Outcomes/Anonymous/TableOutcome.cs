using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public class TableOutcome(Table table) : Outcome(OutcomeKind.Anonymous)
{
    public Table Table = table;
}