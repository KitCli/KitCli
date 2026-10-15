using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>An outcome that lasts the whole session.</summary>
public record TestSessionOutcome() : Outcome(OutcomeLifetime.Session, OutcomeKind.Anonymous);
