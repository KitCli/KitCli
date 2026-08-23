using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="CliCommandNotFoundOutcome"/> by displaying a fixed "Command Not Found" message.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class NotFoundOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome)
        => outcome is CliCommandNotFoundOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        cliIo.Say("Command Not Found");
    }
}