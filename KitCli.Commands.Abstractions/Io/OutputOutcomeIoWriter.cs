using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="FinalSayOutcome"/> by displaying its message.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class OutputOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is FinalSayOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var outputOutcome = (FinalSayOutcome)outcome;
        cliIo.Say(outputOutcome.Something);
    }
}