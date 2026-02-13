namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class NextCliCommandOutcome : CliCommandOutcome
{
    public CliCommand NextCommand { get; }

    public NextCliCommandOutcome(CliCommand nextCommand) : base(CliCommandOutcomeKind.Reusable)
    {
        NextCommand = nextCommand;
    }
}