using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public record AggregatorFilterArtefact(AggregatorFilter Filter) : Artefact<AggregatorFilter>(Filter.FullName, Filter);