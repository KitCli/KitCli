using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

public record AggregatorOutcome<TSource, TAggregate>(Aggregator<TSource, TAggregate> Aggregator) : Outcome(OutcomeKind.Reusable);