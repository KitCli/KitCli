using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorArtefactFactory<TSource, TAggregate> : ArtefactFactory<AggregatorOutcome<TSource, TAggregate>>
{
    protected override AnonymousArtefact CreateArtefact(AggregatorOutcome<TSource, TAggregate> outcome)
        => new AggregatorArtefact<TSource, TAggregate>(outcome.Aggregator);
}