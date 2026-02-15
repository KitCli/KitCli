using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorArtefactFactory<TAggregate> : ArtefactFactory<AggregatorOutcome<TAggregate>>
{
    protected override AnonymousArtefact CreateArtefact(AggregatorOutcome<TAggregate> outcome)
        => new AggregatorArtefact<TAggregate>(outcome.Aggregator);
}