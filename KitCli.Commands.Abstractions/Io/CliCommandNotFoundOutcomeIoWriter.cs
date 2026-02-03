using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class CliCommandNotFoundOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome)
        => outcome is CliCommandNotFoundOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        cliIo.Say("Command Not Found");
    }
}