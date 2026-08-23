using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Outcomes.Anonymous;

/// <summary>
/// Remembers a <see cref="TableBuilder{TSource,TAggregate}"/> so a later "next page" command can rebuild
/// the table without re-supplying its aggregator or map.
/// </summary>
/// <typeparam name="TSource">The type of the table's source elements.</typeparam>
/// <typeparam name="TAggregate">The type of the table's aggregated elements.</typeparam>
/// <param name="TableBuilder">The table builder to remember.</param>
public record TableBuilderOutcome<TSource, TAggregate>(TableBuilder<TSource, TAggregate> TableBuilder)
    : Outcome(OutcomeKind.Reusable);