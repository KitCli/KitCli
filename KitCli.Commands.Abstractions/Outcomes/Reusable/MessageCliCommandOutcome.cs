namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class MessageCliCommandOutcome(string message)
    : CliCommandOutcome(CliCommandOutcomeKind.Reusable)
{
    public string Message { get; } = message;
}