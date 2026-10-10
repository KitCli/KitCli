using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="FinalSayOutcome"/> by displaying its message. A
/// <see cref="LeveledFinalSayOutcome"/> below <paramref name="kitCliSettings"/>'s minimum message level is skipped.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
/// <param name="kitCliSettings">Holds the minimum level a leveled message must meet to be written.</param>
public class OutputOutcomeIoWriter(ICliIo cliIo, KitCliSettings kitCliSettings) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is FinalSayOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var outputOutcome = (FinalSayOutcome)outcome;

        if (outputOutcome is LeveledFinalSayOutcome leveledOutcome && leveledOutcome.Level < kitCliSettings.MinimumMessageLevel)
        {
            return;
        }

        cliIo.Say(outputOutcome.Something);
    }
}