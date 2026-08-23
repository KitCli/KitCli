using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// Remembers an <see cref="Aggregator{TSource,TAggregate}"/> so a later command can query it via its artefact.
/// </summary>
/// <typeparam name="TSource">The type of the aggregator's source elements.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregator's aggregated elements.</typeparam>
/// <param name="Aggregator">The aggregator to remember.</param>
public record AggregatorOutcome<TSource, TAggregate>(Aggregator<TSource, TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);