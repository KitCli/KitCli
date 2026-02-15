namespace KitCli.Commands.Abstractions.Outcomes;

// TODO: Can this be a record?
public abstract class Outcome(OutcomeKind kind)
{
    private OutcomeKind Kind { get; } = kind;

    public bool IsReusable => Kind == OutcomeKind.Reusable;
}