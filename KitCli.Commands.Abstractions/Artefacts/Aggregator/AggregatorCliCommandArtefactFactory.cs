using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator;

public class AggregatorCliCommandArtefactFactory<TAggregate> : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is CliCommandAggregatorOutcome<TAggregate>;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is CliCommandAggregatorOutcome<TAggregate> aggregatorOutcome)
        {
            return new AggregatorCliCommandArtefact<TAggregate>(aggregatorOutcome.Aggregator);
        }
        
        throw new InvalidOperationException(
            $"Cannot create AggregatorCliCommandProperty<{typeof(TAggregate).Name}> from outcome of type {outcome.GetType().Name}");
    }
}