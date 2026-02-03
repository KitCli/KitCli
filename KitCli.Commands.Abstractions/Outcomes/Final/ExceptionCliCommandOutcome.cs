namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class ExceptionCliCommandOutcome(Exception exception) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public Exception Exception { get; set; } = exception;
}