namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A plain message to display, with no effect on the workflow run.
/// </summary>
/// <param name="Something">The message to display.</param>
public record SayOutcome(string Something) : Outcome(OutcomeKind.Anonymous);