namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public class ReactionOutcome() : Outcome(OutcomeKind.Anonymous)
{
    public CliCommandReaction Reaction { get; init; }
}