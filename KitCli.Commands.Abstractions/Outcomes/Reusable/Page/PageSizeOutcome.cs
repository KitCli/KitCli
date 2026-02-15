namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public record PageSizeOutcome(int PageSize) : Outcome(OutcomeKind.Reusable);