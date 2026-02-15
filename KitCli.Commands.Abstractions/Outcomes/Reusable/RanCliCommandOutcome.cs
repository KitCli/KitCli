namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record RanCliCommandOutcome(CliCommand Command) : Outcome(OutcomeKind.Reusable);