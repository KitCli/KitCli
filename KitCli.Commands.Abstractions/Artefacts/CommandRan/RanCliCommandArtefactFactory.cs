using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is RanCliCommandOutcome;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is not RanCliCommandOutcome ranOutcome)
            throw new InvalidOperationException("Cannot create CliCommandRanProperty from the given outcome.");

        return new RanCliCommandArtefact(ranOutcome.Command);
    }
}