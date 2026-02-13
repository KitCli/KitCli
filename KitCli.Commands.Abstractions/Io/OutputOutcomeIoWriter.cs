using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class OutputOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome) => outcome is FinalMessageOutcome;

    public void Write(Outcome outcome)
    {
        var outputOutcome = (FinalMessageOutcome)outcome;
        cliIo.Say(outputOutcome.Output);
    }
}