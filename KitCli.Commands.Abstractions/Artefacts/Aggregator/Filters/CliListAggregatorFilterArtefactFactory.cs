using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterArtefactFactory : ArtefactFactory<FilterOutcome>
{
    protected override AnonymousArtefact CreateArtefact(FilterOutcome outcome)
        => new CliListAggregatorFilterArtefact(outcome.CliListAggregatorFilter);
}