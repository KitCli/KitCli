using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Outputs.Outcomes;

public class ExceptionOutcomeIoWriter(ICliIo cliIo) : IOutcomeIoWriter
{
    public bool CanWriteFor(Outcome outcome)
        => outcome is ExceptionOutcome;

    public void Write(Outcome outcome)
    {
        var exceptionOutcome = (ExceptionOutcome)outcome;
        cliIo.Say(exceptionOutcome.Exception.Message);
    }
}