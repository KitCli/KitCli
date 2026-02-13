using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorCliCommandArtefactFactory<TAggregate> : ICliCommandArtefactFactory
{
    public bool For(Outcome outcome) => outcome is AggregatorOutcome<TAggregate>;

    public CliCommandArtefact Create(Outcome outcome)
    {
        if (outcome is AggregatorOutcome<TAggregate> aggregatorOutcome)
        {
            return new AggregatorCliCommandArtefact<TAggregate>(aggregatorOutcome.Aggregator);
        }
        
        throw new InvalidOperationException(
            $"Cannot create AggregatorCliCommandProperty<{typeof(TAggregate).Name}> from outcome of type {outcome.GetType().Name}");
    }
}