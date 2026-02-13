using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class ListAggregatorOutcome<TAggregate>(CliListAggregator<TAggregate> aggregator)
    : Outcome(OutcomeKind.Reusable)
{
    public CliListAggregator<TAggregate> Aggregator { get; } = aggregator;
}