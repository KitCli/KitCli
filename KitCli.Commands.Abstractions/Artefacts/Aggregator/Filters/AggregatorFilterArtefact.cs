using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

/// <summary>
/// The queryable artefact form of a remembered <see cref="AggregatorFilter"/>, named after the filter's
/// <see cref="AggregatorFilter.FullName"/>.
/// </summary>
/// <param name="Filter">The remembered filter.</param>
public record AggregatorFilterArtefact(AggregatorFilter Filter) : Artefact<AggregatorFilter>(Filter.FullName, Filter);