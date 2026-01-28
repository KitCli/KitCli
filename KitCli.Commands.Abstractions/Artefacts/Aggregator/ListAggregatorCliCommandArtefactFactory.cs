using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class ListAggregatorCliCommandArtefactFactory<TAggregate> : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is ListAggregatorCliCommandOutcome<TAggregate>;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is ListAggregatorCliCommandOutcome<TAggregate> aggregatorOutcome)
        {
            return new ListAggregatorCliCommandArtefact<TAggregate>(aggregatorOutcome.Aggregator);
        }
        
        throw new InvalidOperationException(
            $"Cannot create ListAggregatorCliCommandProperty<{typeof(TAggregate).Name}> from outcome of type {outcome.GetType().Name}");
    }
}