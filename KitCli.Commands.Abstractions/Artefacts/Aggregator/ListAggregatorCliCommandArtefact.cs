using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class ListAggregatorCliCommandArtefact<TAggregate>(CliListAggregator<TAggregate> artefactArtefactValue)
    : ValuedCliCommandArtefact<CliListAggregator<TAggregate>>(typeof(TAggregate).Name, artefactArtefactValue)
{
    
}