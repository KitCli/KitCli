using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

/// <summary>
/// Writes a <see cref="PageSizeOutcome"/> by displaying its page size.
/// </summary>
/// <param name="cliIo">The IO surface to write to.</param>
public class PageSizeOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    /// <inheritdoc/>
    public bool CanWriteFor(Outcome outcome) => outcome is PageSizeOutcome;

    /// <inheritdoc/>
    public void Write(Outcome outcome)
    {
        var pageSizeOutcome = (PageSizeOutcome)outcome;
        cliIo.Say($"Page Size: {pageSizeOutcome.PageSize}");
    }
}