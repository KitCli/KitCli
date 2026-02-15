using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorArtefact<TAggregate>(CliAggregator<TAggregate> value)
    : Artefact<CliAggregator<TAggregate>>(typeof(TAggregate).Name, value);