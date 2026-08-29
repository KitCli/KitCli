namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A reaction to publish, specified by type rather than built. Nothing is constructed when the handler
/// names it: whoever publishes it resolves <see cref="SpecifiedReactionType"/> through the reaction
/// factory path, so the factory sees the run's accumulated artefacts.
/// </summary>
/// <param name="SpecifiedReactionType">The type of the reaction to publish.</param>
public record SpecifiedReactionOutcome(Type SpecifiedReactionType) : Outcome(OutcomeKind.Anonymous);
