namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class CliCommandOutputOutcome(string output) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public string Output = output;
}