namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class FinalMessageOutcome(string output) : Outcome(OutcomeKind.Final)
{
    public string Output = output;
}