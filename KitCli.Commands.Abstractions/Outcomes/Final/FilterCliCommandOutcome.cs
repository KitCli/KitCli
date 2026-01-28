using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Outcomes.Final;

public class FilterCliCommandOutcome : CliCommandOutcome
{
    public FilterCliCommandOutcome(CliListAggregatorFilter cliListAggregatorFilter) : base(CliCommandOutcomeKind.Reusable)
    {
        CliListAggregatorFilter = cliListAggregatorFilter;
    }

    public CliListAggregatorFilter CliListAggregatorFilter { get; }
}