namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

/// <summary>
/// Remembers a page number so a later command can query it via its artefact.
/// </summary>
/// <param name="PageNumber">The page number to remember.</param>
public record PageNumberOutcome(int PageNumber) : Outcome(OutcomeKind.Reusable);