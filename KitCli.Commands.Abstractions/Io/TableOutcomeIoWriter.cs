using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class TableOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is TableOutcome;

    public void Write(Outcome outcome)
    {
        var tableOutcome = (TableOutcome)outcome;
        cliIo.Say(tableOutcome.Table.ToString());
        cliIo.Say($"Results: {tableOutcome.Table.Rows.Count} rows");
    }
}