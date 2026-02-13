using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public class FilterOutcome : Outcome
{
    public FilterOutcome(CliListAggregatorFilter cliListAggregatorFilter) : base(OutcomeKind.Anonymous)
    {
        CliListAggregatorFilter = cliListAggregatorFilter;
    }

    public CliListAggregatorFilter CliListAggregatorFilter { get; }
}