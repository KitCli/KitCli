using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;

/// <summary>
/// Converts an <see cref="AggregatorFilterOutcome"/> into its queryable <see cref="AggregatorFilterArtefact"/> form.
/// </summary>
public class AggregatorFilterArtefactFactory : ArtefactFactory<AggregatorFilterOutcome>
{
    /// <inheritdoc/>
    protected override AnonymousArtefact CreateArtefact(AggregatorFilterOutcome outcome)
        => new AggregatorFilterArtefact(outcome.AggregateFilter);
}