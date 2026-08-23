namespace KitCli.Commands.Abstractions.Outcomes.Final;

/// <summary>
/// Ends the workflow run because an exception occurred while handling a command.
/// </summary>
/// <param name="Exception">The exception that occurred.</param>
public record ExceptionOutcome(Exception Exception) : Outcome(OutcomeKind.Final);