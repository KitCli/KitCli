namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public class SayOutcome(string message) : Outcome(OutcomeKind.Anonymous)
{
    public string Message { get; } = message;
}