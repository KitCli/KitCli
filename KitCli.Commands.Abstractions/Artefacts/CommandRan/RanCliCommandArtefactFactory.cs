using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Skippable;

namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(Outcome outcome) => outcome is RanOutcome;

    public CliCommandArtefact Create(Outcome outcome)
    {
        if (outcome is not RanOutcome ranOutcome)
            throw new InvalidOperationException("Cannot create CliCommandRanProperty from the given outcome.");

        return new RanCliCommandArtefact(ranOutcome.Command);
    }
}