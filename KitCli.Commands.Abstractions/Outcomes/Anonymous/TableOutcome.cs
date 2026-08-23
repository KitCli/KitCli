using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// A table to display once, with no effect on the workflow run.
/// </summary>
/// <param name="Table">The table to display.</param>
public record TableOutcome(Table Table) : Outcome(OutcomeKind.Anonymous);