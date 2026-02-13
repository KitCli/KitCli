using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Io;

public class PageNumberOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is PageNumberOutcome;

    public void Write(Outcome outcome)
    {
        var pageNumberOutcome = (PageNumberOutcome)outcome;
        cliIo.Say($"Page Number: {pageNumberOutcome.PageNumber}");
    }
}