using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// Records that an <see cref="AggregatorFilter"/> was applied, so a later command can read what was applied
/// via its artefact. Advisory only — nothing ties it back to the actual filtering function passed to an aggregator.
/// </summary>
/// <param name="AggregateFilter">The filter that was applied.</param>
public record AggregatorFilterOutcome(AggregatorFilter AggregateFilter) : Outcome(OutcomeKind.Anonymous);