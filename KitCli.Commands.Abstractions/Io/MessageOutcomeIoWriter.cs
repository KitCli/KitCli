using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="SayOutcome"/> by displaying its message. A <see cref="LeveledSayOutcome"/>
/// below <paramref name="kitCliSettings"/>'s minimum message level is skipped.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
/// <param name="kitCliSettings">Holds the minimum level a leveled message must meet to be written.</param>
public class MessageOutcomeIoWriter(ICliIo cliIo, KitCliSettings kitCliSettings) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is SayOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var messageOutcome = (SayOutcome)outcome;

        if (messageOutcome is LeveledSayOutcome leveledOutcome && leveledOutcome.Level < kitCliSettings.MinimumMessageLevel)
        {
            return;
        }

        cliIo.Say(messageOutcome.Something);
    }
}