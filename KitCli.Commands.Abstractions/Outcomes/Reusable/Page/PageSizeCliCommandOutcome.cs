namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class PageSizeCliCommandOutcome(int pageSize) : CliCommandOutcome(CliCommandOutcomeKind.Reusable)
{
    public int PageSize { get; } = pageSize;
}