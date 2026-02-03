using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Outputs.Outcomes;

public class ExceptionCliCommandOutcomeIoWriter(ICliIo cliIo) : ICliCommandOutcomeIoWriter
{
    public bool CanWriteFor(CliCommandOutcome outcome)
        => outcome is ExceptionCliCommandOutcome;

    public void Write(CliCommandOutcome outcome)
    {
        var exceptionOutcome = (ExceptionCliCommandOutcome)outcome;
        cliIo.Say(exceptionOutcome.Exception.Message);
    }
}