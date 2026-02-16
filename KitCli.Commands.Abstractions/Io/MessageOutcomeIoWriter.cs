using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Io;

public class MessageOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is SayOutcome;

    public void Write(Outcome outcome)
    {
        var messageOutcome = (SayOutcome)outcome;
        cliIo.Say(messageOutcome.Something);
    }
}