namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public class PageNumberOutcome(int pageNumber) : Outcome(OutcomeKind.Reusable)
{
    public int PageNumber { get; } = pageNumber;
}