using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class ListAggregatorUnvaluedArtefact<TAggregate>(CliListAggregator<TAggregate> value)
    : Artefact<CliListAggregator<TAggregate>>(typeof(TAggregate).Name, value)
{
    
}