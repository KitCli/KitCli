using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorUnvaluedArtefact<TAggregate>(CliAggregator<TAggregate> value)
    : Artefact<CliAggregator<TAggregate>>(typeof(TAggregate).Name, value);