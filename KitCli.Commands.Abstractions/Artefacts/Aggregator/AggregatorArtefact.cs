using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public record AggregatorArtefact<TAggregate>(Aggregator<TAggregate> Value)
    : Artefact<Aggregator<TAggregate>>(typeof(TAggregate).Name, Value);