using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class ListAggregatorCliCommandOutcome<TAggregate>(CliListAggregator<TAggregate> aggregator)
    : CliCommandOutcome(CliCommandOutcomeKind.Reusable)
{
    public CliListAggregator<TAggregate> Aggregator { get; } = aggregator;
}