using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is PageNumberCliCommandOutcome;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is not PageNumberCliCommandOutcome pageNumberOutcome)
            throw new InvalidOperationException("Cannot create PageNumberCliCommandArtefact from the given outcome.");

        return new PageNumberCliCommandArtefact(pageNumberOutcome.PageNumber);
    }
}