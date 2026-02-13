using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorArtefactFactory<TAggregate> : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is AggregatorOutcome<TAggregate>;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is AggregatorOutcome<TAggregate> aggregatorOutcome)
        {
            return new AggregatorUnvaluedArtefact<TAggregate>(aggregatorOutcome.Aggregator);
        }
        
        throw new InvalidOperationException(
            $"Cannot create AggregatorCliCommandProperty<{typeof(TAggregate).Name}> from outcome of type {outcome.GetType().Name}");
    }
}