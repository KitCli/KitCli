using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record AggregatorOutcome<TAggregate>(Aggregator<TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);