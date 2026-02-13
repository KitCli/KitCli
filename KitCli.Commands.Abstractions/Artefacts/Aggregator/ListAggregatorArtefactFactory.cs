using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class ListAggregatorArtefactFactory<TAggregate> : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is ListAggregatorOutcome<TAggregate>;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is ListAggregatorOutcome<TAggregate> aggregatorOutcome)
        {
            return new ListAggregatorUnvaluedArtefact<TAggregate>(aggregatorOutcome.Aggregator);
        }
        
        throw new InvalidOperationException(
            $"Cannot create ListAggregatorCliCommandProperty<{typeof(TAggregate).Name}> from outcome of type {outcome.GetType().Name}");
    }
}