using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public class AggregatorOutcome<TAggregate>(CliAggregator<TAggregate> aggregator)
    : Outcome(OutcomeKind.Reusable)
{
    public CliAggregator<TAggregate> Aggregator { get; } = aggregator;
}