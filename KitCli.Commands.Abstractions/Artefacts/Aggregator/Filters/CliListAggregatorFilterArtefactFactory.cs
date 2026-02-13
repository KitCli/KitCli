using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterArtefactFactory : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is FilterOutcome;

    public AnonymousArtefact Create(Outcome outcome)
    {
        var filteredOutcome = (FilterOutcome)outcome;
        return new CliListAggregatorFilterAnonymousArtefact(filteredOutcome.CliListAggregatorFilter);
    }
}