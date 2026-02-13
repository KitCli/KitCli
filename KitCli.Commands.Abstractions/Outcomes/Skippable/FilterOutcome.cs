using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Outcomes.Skippable;

public class FilterOutcome : Outcome
{
    public FilterOutcome(CliListAggregatorFilter cliListAggregatorFilter) : base(OutcomeKind.Skippable)
    {
        CliListAggregatorFilter = cliListAggregatorFilter;
    }

    public CliListAggregatorFilter CliListAggregatorFilter { get; }
}