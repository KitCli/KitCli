namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class OutputCliCommandOutcome(string output) : CliCommandOutcome(CliCommandOutcomeKind.Final)
{
    public string Output = output;
}