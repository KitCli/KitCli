using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class TableCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome) => outcome is TableCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var tableOutcome = (TableCliCommandOutcome)outcome;
        cliIo.Say(tableOutcome.Table.ToString());
        cliIo.Say($"Results: {tableOutcome.Table.Rows.Count} rows");
    }
}