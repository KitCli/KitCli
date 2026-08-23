namespace KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

/// <summary>
/// Remembers a page size so a later command can query it via its artefact.
/// </summary>
/// <param name="PageSize">The page size to remember.</param>
public record PageSizeOutcome(int PageSize) : Outcome(OutcomeKind.Reusable);