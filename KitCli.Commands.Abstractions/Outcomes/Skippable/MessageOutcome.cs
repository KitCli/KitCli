namespace KitCli.Commands.Abstractions.Outcomes.Skippable;

public class MessageOutcome(string message) : Outcome(OutcomeKind.Skippable)
{
    public string Message { get; } = message;
}