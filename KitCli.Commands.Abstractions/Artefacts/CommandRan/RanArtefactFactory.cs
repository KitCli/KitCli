using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanArtefactFactory : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is RanOutcome;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is not RanOutcome ranOutcome)
            throw new InvalidOperationException("Cannot create CliCommandRanProperty from the given outcome.");

        return new RanCliCommandAnonymousArtefact(ranOutcome.Command);
    }
}