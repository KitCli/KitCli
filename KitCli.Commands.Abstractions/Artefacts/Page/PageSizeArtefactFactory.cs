using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;

namespace KitCli.Commands.Abstractions.Artefacts.Page;

/// <summary>
/// Converts a <see cref="PageSizeOutcome"/> into its queryable <see cref="PageSizeArtefact"/> form.
/// </summary>
public class PageSizeArtefactFactory : ArtefactFactory<PageSizeOutcome>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(PageSizeOutcome outcome)
        => new PageSizeArtefact(outcome.PageSize);
}