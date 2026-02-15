using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record AggregatorFilterOutcome(AnonymousAggregatorFilter AggregateFilter) : Outcome(OutcomeKind.Anonymous);