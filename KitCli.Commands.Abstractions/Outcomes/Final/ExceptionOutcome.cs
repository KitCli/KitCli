namespace KitCli.Commands.Abstractions.Outcomes.Final;

public record ExceptionOutcome(Exception Exception) : Outcome(OutcomeKind.Final);