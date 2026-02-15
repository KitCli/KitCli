using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public record AggregatorArtefact<TAggregate>(CliAggregator<TAggregate> Value)
    : Artefact<CliAggregator<TAggregate>>(typeof(TAggregate).Name, Value);