namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class ExceptionOutcome(Exception exception) : Outcome(OutcomeKind.Final)
{
    public Exception Exception { get; set; } = exception;
}