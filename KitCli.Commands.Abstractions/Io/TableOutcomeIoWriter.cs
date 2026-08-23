using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="TableOutcome"/> by displaying its rendered table.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class TableOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is TableOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var tableOutcome = (TableOutcome)outcome;
        cliIo.Say(tableOutcome.Table.ToString());
    }
}