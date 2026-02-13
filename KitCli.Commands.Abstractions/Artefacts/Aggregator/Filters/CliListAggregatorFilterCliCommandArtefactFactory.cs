using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Skippable;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(Outcome outcome) => outcome is FilterOutcome;

    public CliCommandArtefact Create(Outcome outcome)
    {
        var filteredOutcome = (FilterOutcome)outcome;
        return new CliListAggregatorFilterCliCommandArtefact(filteredOutcome.CliListAggregatorFilter);
    }
}