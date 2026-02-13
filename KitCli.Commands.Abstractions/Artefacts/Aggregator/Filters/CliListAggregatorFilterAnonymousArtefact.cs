using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterAnonymousArtefact : AnonymousArtefact
{
    public CliListAggregatorFilter CliListAggregatorFilter { get; }

    public CliListAggregatorFilterAnonymousArtefact(CliListAggregatorFilter cliListAggregatorFilter) 
        : base($"{cliListAggregatorFilter.FilterName}-{cliListAggregatorFilter.FilterFieldName}")
    {
        CliListAggregatorFilter = cliListAggregatorFilter;
    }
}