using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

public class PageNumberCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome) => outcome is PageNumberCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var pageNumberOutcome = (PageNumberCliCommandOutcome)outcome;
        cliIo.Say($"Page Number: {pageNumberOutcome.PageNumber}");
    }
}