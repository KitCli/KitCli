using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(Outcome outcome) => outcome is PageNumberOutcome;

    public CliCommandArtefact Create(Outcome outcome)
    {
        if (outcome is not PageNumberOutcome pageNumberOutcome)
            throw new InvalidOperationException("Cannot create PageNumberCliCommandArtefact from the given outcome.");

        return new PageNumberCliCommandArtefact(pageNumberOutcome.PageNumber);
    }
}