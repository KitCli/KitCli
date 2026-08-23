using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

/// <summary>
/// Converts a <see cref="PageNumberOutcome"/> into its queryable <see cref="PageNumberArtefact"/> form.
/// </summary>
public class PageNumberArtefactFactory : ArtefactFactory<PageNumberOutcome>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(PageNumberOutcome outcome)
        => new PageNumberArtefact(outcome.PageNumber);
}