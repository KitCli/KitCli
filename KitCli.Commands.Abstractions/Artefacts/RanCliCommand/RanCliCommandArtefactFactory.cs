using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.RanCliCommand;

/// <summary>
/// Converts a <see cref="RanCliCommandOutcome"/> into its queryable <see cref="RanCliCommandArtefact"/> form.
/// </summary>
public class RanCliCommandArtefactFactory : ArtefactFactory<RanCliCommandOutcome>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(RanCliCommandOutcome cliCommandOutcome)
        => new RanCliCommandArtefact(cliCommandOutcome.Command);
}