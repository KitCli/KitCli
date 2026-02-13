using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(Outcome outcome) => outcome is PageSizeOutcome;

    public CliCommandArtefact Create(Outcome outcome)
    {
        if (outcome is not PageSizeOutcome pageSizeOutcome)
            throw new InvalidOperationException("Cannot create PageSizeCliCommandArtefact from the given outcome.");
        
        return new PageSizeCliCommandArtefact(pageSizeOutcome.PageSize);
    }
}