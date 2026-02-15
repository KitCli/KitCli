using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public record ListAggregatorArtefact<TAggregate>(CliListAggregator<TAggregate> Value)
    : Artefact<CliListAggregator<TAggregate>>(typeof(TAggregate).Name, Value);