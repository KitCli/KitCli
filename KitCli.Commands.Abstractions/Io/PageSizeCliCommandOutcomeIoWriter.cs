using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

public class PageSizeCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome) => outcome is PageSizeCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var pageSizeOutcome = (PageSizeCliCommandOutcome)outcome;
        cliIo.Say($"Page Size: {pageSizeOutcome.PageSize}");
    }
}