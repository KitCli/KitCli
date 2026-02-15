namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record ReactionOutcome(CliCommandReaction Reaction) : Outcome(OutcomeKind.Anonymous);