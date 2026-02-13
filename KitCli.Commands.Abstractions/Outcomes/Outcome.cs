namespace KitCli.Commands.Abstractions.Outcomes;

public abstract class Outcome(OutcomeKind kind)
{
    private OutcomeKind Kind { get; } = kind;

    public bool IsReusable => Kind == OutcomeKind.Reusable;
}