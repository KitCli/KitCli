using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class AggregatorFilterArtefactFactory : ArtefactFactory<AggregatorFilterOutcome>
{
    protected override AnonymousArtefact CreateArtefact(AggregatorFilterOutcome outcome)
        => new AggregatorFilterArtefact(outcome.AggregateFilter);
}