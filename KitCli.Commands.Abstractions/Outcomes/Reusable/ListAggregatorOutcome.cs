using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record ListAggregatorOutcome<TAggregate>(ListAggregator<TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);