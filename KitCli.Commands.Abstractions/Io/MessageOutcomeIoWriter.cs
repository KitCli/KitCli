using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="SayOutcome"/> by displaying its message.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class MessageOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is SayOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var messageOutcome = (SayOutcome)outcome;
        cliIo.Say(messageOutcome.Something);
    }
}