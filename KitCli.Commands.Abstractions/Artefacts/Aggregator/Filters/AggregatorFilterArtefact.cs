using KitCli.Abstractions.Aggregators.Filters;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public record AggregatorFilterArtefact(AggregatorFilter Filter)
    : Artefact<AggregatorFilter>(Filter.FullName, Filter);