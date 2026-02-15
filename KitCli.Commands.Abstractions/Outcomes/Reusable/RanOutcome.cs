namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record RanOutcome(CliCommand Command) : Outcome(OutcomeKind.Reusable);