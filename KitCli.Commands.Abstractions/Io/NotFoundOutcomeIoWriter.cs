using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class NotFoundOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome)
        => outcome is CliCommandNotFoundOutcome;

    public void Write(Outcome outcome)
    {
        cliIo.Say("Command Not Found");
    }
}