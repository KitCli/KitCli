using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberArtefactFactory : ArtefactFactory<PageNumberOutcome>
{
    protected override AnonymousArtefact CreateArtefact(PageNumberOutcome outcome)
        => new PageNumberUnvaluedArtefact(outcome.PageNumber);
}