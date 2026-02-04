using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Io;

public class MessageCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome) => outcome is MessageCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var messageOutcome = (MessageCliCommandOutcome)outcome;
        cliIo.Say(messageOutcome.Message);
    }
}