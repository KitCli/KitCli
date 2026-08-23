namespace KitCli.Commands.Abstractions.Outcomes.Final;

/// <summary>
/// Ends the workflow run without displaying anything further.
/// </summary>
public record NothingOutcome() : Outcome(OutcomeKind.Final);