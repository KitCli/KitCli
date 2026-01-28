using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterCliCommandArtefact : CliCommandArtefact
{
    public CliListAggregatorFilter CliListAggregatorFilter { get; }

    public CliListAggregatorFilterCliCommandArtefact(CliListAggregatorFilter cliListAggregatorFilter) 
        : base($"{cliListAggregatorFilter.FilterName}-{cliListAggregatorFilter.FilterFieldName}")
    {
        CliListAggregatorFilter = cliListAggregatorFilter;
    }
}