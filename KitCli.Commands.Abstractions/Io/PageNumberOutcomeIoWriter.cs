using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="PageNumberOutcome"/> by displaying its page number.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class PageNumberOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is PageNumberOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var pageNumberOutcome = (PageNumberOutcome)outcome;
        cliIo.Say($"Page Number: {pageNumberOutcome.PageNumber}");
    }
}