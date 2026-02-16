using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.TableBuilder;

public class TableBuilderArtefactFactory<TSource, TAggregate> : ArtefactFactory<TableBuilderOutcome<TSource, TAggregate>>
{
    protected override AnonymousArtefact CreateArtefact(TableBuilderOutcome<TSource, TAggregate> outcome)
        => new TableBuilderArtefact<TSource, TAggregate>(outcome.TableBuilder);
}