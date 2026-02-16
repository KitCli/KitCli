using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Artefacts.TableBuilder;

public record TableBuilderArtefact<TSource, TAggregate>(TableBuilder<TSource, TAggregate> TableBuilder)
    : Artefact<TableBuilder<TSource, TAggregate>>(TableBuilder.GetType().Name, TableBuilder);