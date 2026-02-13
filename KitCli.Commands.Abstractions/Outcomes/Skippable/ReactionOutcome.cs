namespace KitCli.Commands.Abstractions.Outcomes.Skippable;

public class ReactionOutcome() : Outcome(OutcomeKind.Skippable)
{
    public CliCommandReaction Reaction { get; init; }
}