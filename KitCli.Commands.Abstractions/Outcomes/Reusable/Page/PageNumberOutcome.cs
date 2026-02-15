namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

public record PageNumberOutcome(int PageNumber) : Outcome(OutcomeKind.Reusable);