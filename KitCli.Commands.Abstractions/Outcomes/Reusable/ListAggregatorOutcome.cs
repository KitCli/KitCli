using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record ListAggregatorOutcome<TAggregate>(CliListAggregator<TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);