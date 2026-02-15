using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public record ListAggregatorArtefact<TAggregate>(ListAggregator<TAggregate> Value)
    : Artefact<ListAggregator<TAggregate>>(typeof(TAggregate).Name, Value);