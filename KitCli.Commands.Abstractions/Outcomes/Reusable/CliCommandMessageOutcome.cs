namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class CliCommandMessageOutcome(string message)
    : CliCommandOutcome(CliCommandOutcomeKind.Reusable)
{
    public string Message { get; } = message;
}