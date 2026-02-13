namespace KitCli.Commands.Abstractions.Outcomes.Skippable;

public class ReactionCliCommandOutcome() : CliCommandOutcome(CliCommandOutcomeKind.Skippable)
{
    public CliCommandReaction Reaction { get; init; }
}