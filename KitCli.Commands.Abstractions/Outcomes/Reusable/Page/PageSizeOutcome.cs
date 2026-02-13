namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class PageSizeOutcome(int pageSize) : Outcome(OutcomeKind.Reusable)
{
    public int PageSize { get; } = pageSize;
}