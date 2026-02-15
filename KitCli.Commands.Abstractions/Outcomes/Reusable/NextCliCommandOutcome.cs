namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record NextCliCommandOutcome(CliCommand NextCommand) : Outcome(OutcomeKind.Reusable);