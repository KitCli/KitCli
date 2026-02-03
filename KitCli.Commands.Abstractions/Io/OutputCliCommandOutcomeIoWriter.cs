using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Io;

public class OutputCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome) => outcome is OutputCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var outputOutcome = (OutputCliCommandOutcome)outcome;
        cliIo.Say(outputOutcome.Output);
    }
}