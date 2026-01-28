using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

public class CliListAggregatorFilterCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is FilterCliCommandOutcome;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        var filteredOutcome = (FilterCliCommandOutcome)outcome;
        return new CliListAggregatorFilterCliCommandArtefact(filteredOutcome.CliListAggregatorFilter);
    }
}