namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// Carries a <see cref="CliCommandReaction"/> to be published as a side effect of a command running.
/// </summary>
/// <param name="Reaction">The reaction to publish.</param>
public record ReactionOutcome(CliCommandReaction Reaction) : Outcome(OutcomeKind.Anonymous);