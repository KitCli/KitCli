using KitCli.Abstractions.Aggregators.Filters;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record FilterOutcome(CliListAggregatorFilter CliListAggregatorFilter) : Outcome(OutcomeKind.Anonymous);