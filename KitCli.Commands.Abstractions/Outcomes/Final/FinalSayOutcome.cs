namespace KitCli.Commands.Abstractions.Outcomes.Final;

/// <summary>
/// A closing message to display, ending the workflow run.
/// </summary>
/// <param name="Something">The message to display.</param>
public record FinalSayOutcome(string Something) : Outcome(OutcomeKind.Final);