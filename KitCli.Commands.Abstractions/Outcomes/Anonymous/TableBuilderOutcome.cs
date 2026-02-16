using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

public record TableBuilderOutcome<TSource, TAggregate>(TableBuilder<TSource, TAggregate> TableBuilder) 
    : Outcome(OutcomeKind.Reusable);