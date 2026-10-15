using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Tests.TestHelpers;

/// <summary>An outcome that lasts the whole session, telling instances apart by what it carries.</summary>
public record TestSessionOutcome(string Text) : Outcome(OutcomeLifetime.Session, OutcomeKind.Anonymous);
