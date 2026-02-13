namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// Identifies a command that previously ran. 
/// </summary>
/// <param name="command"></param>
public class RanOutcome(CliCommand command) : Outcome(OutcomeKind.Reusable)
{
    public CliCommand Command { get; } = command;
}