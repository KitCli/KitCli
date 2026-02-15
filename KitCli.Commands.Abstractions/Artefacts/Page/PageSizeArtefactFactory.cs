using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeArtefactFactory : ArtefactFactory<PageSizeOutcome>
{
    protected override AnonymousArtefact CreateArtefact(PageSizeOutcome outcome)
        => new PageSizeArtefact(outcome.PageSize);
}