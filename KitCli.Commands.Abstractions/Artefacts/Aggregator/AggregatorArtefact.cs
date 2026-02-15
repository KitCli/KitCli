using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public record AggregatorArtefact<TSource, TAggregate>(Aggregator<TSource, TAggregate> Value)
    : Artefact<Aggregator<TSource, TAggregate>>(Value.GetType().Name, Value);