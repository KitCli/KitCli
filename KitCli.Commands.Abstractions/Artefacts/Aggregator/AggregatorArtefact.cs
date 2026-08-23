using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

/// <summary>
/// The queryable artefact form of a remembered <see cref="Aggregator{TSource,TAggregate}"/>, named after its
/// concrete type so a later command factory can retrieve it by that closed generic type.
/// </summary>
/// <typeparam name="TSource">The type of the source elements the aggregator operates on.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated elements the aggregator produces.</typeparam>
/// <param name="Value">The remembered aggregator instance.</param>
public record AggregatorArtefact<TSource, TAggregate>(Aggregator<TSource, TAggregate> Value)
    : Artefact<Aggregator<TSource, TAggregate>>(Value.GetType().Name, Value);