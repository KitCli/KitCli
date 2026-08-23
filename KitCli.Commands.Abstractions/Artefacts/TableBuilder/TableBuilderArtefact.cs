using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Artefacts.TableBuilder;

/// <summary>
/// The queryable artefact form of a remembered <see cref="TableBuilder{TSource,TAggregate}"/>, named after its
/// concrete type so a later "next page" command factory can rebuild the table without re-supplying its
/// aggregator or map.
/// </summary>
/// <typeparam name="TSource">The type of the source elements the table's aggregator operates on.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated elements the table's rows are built from.</typeparam>
/// <param name="TableBuilder">The remembered table builder instance.</param>
public record TableBuilderArtefact<TSource, TAggregate>(TableBuilder<TSource, TAggregate> TableBuilder)
    : Artefact<TableBuilder<TSource, TAggregate>>(TableBuilder.GetType().Name, TableBuilder);