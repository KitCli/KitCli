using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeArtefactFactory : IArtefactFactory
{
    public bool For(Outcome outcome) => outcome is PageSizeOutcome;

    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is not PageSizeOutcome pageSizeOutcome)
            throw new InvalidOperationException("Cannot create PageSizeCliCommandArtefact from the given outcome.");
        
        return new PageSizeUnvaluedArtefact(pageSizeOutcome.PageSize);
    }
}