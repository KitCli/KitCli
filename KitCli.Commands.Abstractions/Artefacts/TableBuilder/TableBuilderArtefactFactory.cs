using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.TableBuilder;

/// <summary>
/// Converts a <see cref="TableBuilderOutcome{TSource,TAggregate}"/> into its queryable <see cref="TableBuilderArtefact{TSource,TAggregate}"/> form.
/// </summary>
/// <typeparam name="TSource">The type of the source elements the table's aggregator operates on.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated elements the table's rows are built from.</typeparam>
public class TableBuilderArtefactFactory<TSource, TAggregate> : ArtefactFactory<TableBuilderOutcome<TSource, TAggregate>>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(TableBuilderOutcome<TSource, TAggregate> outcome)
        => new TableBuilderArtefact<TSource, TAggregate>(outcome.TableBuilder);
}