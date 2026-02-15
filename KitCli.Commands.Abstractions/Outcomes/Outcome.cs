namespace KitCli.Commands.Abstractions.Outcomes;

public abstract record Outcome(OutcomeKind Kind)
{
    public bool IsReusable => Kind == OutcomeKind.Reusable;
}