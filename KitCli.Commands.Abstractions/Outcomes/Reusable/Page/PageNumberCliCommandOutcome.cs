namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class PageNumberCliCommandOutcome(int pageNumber) : CliCommandOutcome(CliCommandOutcomeKind.Reusable)
{
    public int PageNumber { get; } = pageNumber;
}