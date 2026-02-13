using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberArtefactFactory : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is PageNumberOutcome;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is not PageNumberOutcome pageNumberOutcome)
            throw new InvalidOperationException("Cannot create PageNumberCliCommandArtefact from the given outcome.");

        return new PageNumberUnvaluedArtefact(pageNumberOutcome.PageNumber);
    }
}