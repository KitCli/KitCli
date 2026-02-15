namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record SayOutcome(string Something) : Outcome(OutcomeKind.Anonymous);