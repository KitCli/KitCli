using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

public class PageSizeOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is PageSizeOutcome;

    public void Write(Outcome outcome)
    {
        var pageSizeOutcome = (PageSizeOutcome)outcome;
        cliIo.Say($"Page Size: {pageSizeOutcome.PageSize}");
    }
}