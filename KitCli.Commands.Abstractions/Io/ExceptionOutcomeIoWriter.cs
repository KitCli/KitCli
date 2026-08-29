using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes an <see cref="ExceptionOutcome"/> by displaying its exception's message.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class ExceptionOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome)
        => outcome is ExceptionOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var exceptionOutcome = (ExceptionOutcome)outcome;
        cliIo.Say(exceptionOutcome.Exception.Message);
    }
}