namespace KitCli.Commands.Abstractions.Outcomes;

public abstract class CliCommandOutcome(CliCommandOutcomeKind kind)
{
    private CliCommandOutcomeKind Kind { get; } = kind;

    public bool IsReusable => Kind == CliCommandOutcomeKind.Reusable;
}