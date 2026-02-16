using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record AggregatorFilterOutcome(AggregatorFilter AggregateFilter) : Outcome(OutcomeKind.Anonymous);