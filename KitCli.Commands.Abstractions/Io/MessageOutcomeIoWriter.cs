using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Skippable;

namespace KitCli.Commands.Abstractions.Io;

public class MessageOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is MessageOutcome;

    public void Write(Outcome outcome)
    {
        var messageOutcome = (MessageOutcome)outcome;
        cliIo.Say(messageOutcome.Message);
    }
}