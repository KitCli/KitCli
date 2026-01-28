using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorCliCommandArtefact<TAggregate>(CliAggregator<TAggregate> artefactArtefactValue)
    : ValuedCliCommandArtefact<CliAggregator<TAggregate>>(typeof(TAggregate).Name, artefactArtefactValue);