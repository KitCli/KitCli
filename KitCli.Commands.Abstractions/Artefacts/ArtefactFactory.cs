using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

/// <summary>
/// Base class for a factory that converts one specific <see cref="Outcome"/> type into its queryable
/// <see cref="AnonymousArtefact"/> form. Implementers only need to provide <see cref="CreateArtefact"/>;
/// the outcome-type matching required by <see cref="IArtefactFactory.For"/> is handled here.
/// </summary>
/// <typeparam name="TOutcome">The outcome type this factory converts into an artefact.</typeparam>
public abstract class ArtefactFactory<TOutcome> : IArtefactFactory where TOutcome : Outcome
{
    /// <summary>
    /// Determines whether this factory can convert the given outcome, based on whether it is a <typeparamref name="TOutcome"/>.
    /// </summary>
    /// <param name="outcome">The outcome to check.</param>
    /// <returns><see langword="true"/> if <paramref name="outcome"/> is a <typeparamref name="TOutcome"/>; otherwise <see langword="false"/>.</returns>
    public bool For(Outcome outcome) => outcome is TOutcome;

    /// <summary>
    /// Converts the given outcome into its artefact form by delegating to <see cref="CreateArtefact"/>.
    /// </summary>
    /// <param name="outcome">The outcome to convert. Must be a <typeparamref name="TOutcome"/>.</param>
    /// <returns>The artefact created from <paramref name="outcome"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="outcome"/> is not a <typeparamref name="TOutcome"/>.
    /// </exception>
    public AnonymousArtefact Create(Outcome outcome)
    {
        if (outcome is TOutcome typedOutcome)
        {
            return CreateArtefact(typedOutcome);
        }

        // TODO: Custom exception to be handled somewhere else.
        throw new InvalidOperationException(
            $"Cannot create artefact from outcome of type {outcome.GetType().Name} using factory for {typeof(TOutcome).Name}");
    }

    /// <summary>
    /// Creates the artefact form of the given outcome.
    /// </summary>
    /// <param name="outcome">The outcome to convert.</param>
    /// <returns>The artefact created from <paramref name="outcome"/>.</returns>
    protected abstract AnonymousArtefact CreateArtefact(TOutcome outcome);
}