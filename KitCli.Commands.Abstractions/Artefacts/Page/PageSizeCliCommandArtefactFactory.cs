using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeCliCommandArtefactFactory : ICliCommandArtefactFactory
{
    public bool For(CliCommandOutcome outcome) => outcome is PageSizeCliCommandOutcome;

    public CliCommandArtefact Create(CliCommandOutcome outcome)
    {
        if (outcome is not PageSizeCliCommandOutcome pageSizeOutcome)
            throw new InvalidOperationException("Cannot create PageSizeCliCommandArtefact from the given outcome.");
        
        return new PageSizeCliCommandArtefact(pageSizeOutcome.PageSize);
    }
}