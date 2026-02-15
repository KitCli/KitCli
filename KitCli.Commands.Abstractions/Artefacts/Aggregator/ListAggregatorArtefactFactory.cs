using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class ListAggregatorArtefactFactory<TAggregate> : ArtefactFactory<ListAggregatorOutcome<TAggregate>>
{
    protected override AnonymousArtefact CreateArtefact(ListAggregatorOutcome<TAggregate> outcome)
        => new ListAggregatorUnvaluedArtefact<TAggregate>(outcome.Aggregator);
}