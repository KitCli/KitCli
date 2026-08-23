using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

/// <summary>
/// Converts a <see cref="Outcome"/> produced by a command into the queryable <see cref="AnonymousArtefact"/>
/// form that later commands' factories can look up by type and name.
/// </summary>
public interface IArtefactFactory
{
    /// <summary>
    /// Determines whether this factory can convert the given outcome.
    /// </summary>
    /// <param name="outcome">The outcome to check.</param>
    /// <returns><see langword="true"/> if this factory can convert <paramref name="outcome"/>; otherwise <see langword="false"/>.</returns>
    bool For(Outcome outcome);

    /// <summary>
    /// Converts the given outcome into its artefact form.
    /// </summary>
    /// <param name="outcome">The outcome to convert.</param>
    /// <returns>The artefact created from <paramref name="outcome"/>.</returns>
    AnonymousArtefact Create(Outcome outcome);
}