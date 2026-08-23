using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

/// <summary>
/// Converts an <see cref="AggregatorOutcome{TSource,TAggregate}"/> into its queryable <see cref="AggregatorArtefact{TSource,TAggregate}"/> form.
/// </summary>
/// <typeparam name="TSource">The type of the source elements the aggregator operates on.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated elements the aggregator produces.</typeparam>
public class AggregatorArtefactFactory<TSource, TAggregate> : ArtefactFactory<AggregatorOutcome<TSource, TAggregate>>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(AggregatorOutcome<TSource, TAggregate> outcome)
        => new AggregatorArtefact<TSource, TAggregate>(outcome.Aggregator);
}