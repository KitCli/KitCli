namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class NextCliCommandOutcome : Outcome
{
    public CliCommand NextCommand { get; }

    public NextCliCommandOutcome(CliCommand nextCommand) : base(OutcomeKind.Reusable)
    {
        NextCommand = nextCommand;
    }
}