using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="PauseOutcome"/> by displaying a blank line.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class PauseOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is PauseOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome) => cliIo.Pause();
}
