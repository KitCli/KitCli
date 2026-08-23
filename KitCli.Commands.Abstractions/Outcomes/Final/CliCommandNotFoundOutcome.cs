namespace KitCli.Commands.Abstractions.Outcomes.Final;

/// <summary>
/// Ends the workflow run because no command was found for the user's ask.
/// </summary>
public record CliCommandNotFoundOutcome() : Outcome(OutcomeKind.Final);