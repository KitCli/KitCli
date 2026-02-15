using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record AggregatorOutcome<TAggregate>(CliAggregator<TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);