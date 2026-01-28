namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class CliCommandExceptionOutcome(Exception exception) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public Exception Exception { get; set; } = exception;
}