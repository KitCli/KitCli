namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A blank line to display, with no effect on the workflow run.
/// </summary>
public record PauseOutcome() : Outcome(OutcomeKind.Anonymous);
