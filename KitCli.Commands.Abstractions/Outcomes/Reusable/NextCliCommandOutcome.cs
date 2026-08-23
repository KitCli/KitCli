namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// Remembers which command should run next.
/// </summary>
/// <param name="NextCommand">The command to move to.</param>
public record NextCliCommandOutcome(CliCommand NextCommand) : Outcome(OutcomeKind.Reusable);