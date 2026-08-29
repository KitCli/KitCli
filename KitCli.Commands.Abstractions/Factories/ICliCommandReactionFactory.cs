using KitCli.Commands.Abstractions.Artefacts;

namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// Builds a <see cref="CliCommandReaction"/> instance for a specified reaction type, optionally using
/// the run's artefacts.
/// </summary>
public interface ICliCommandReactionFactory
{
    /// <summary>
    /// Attaches the artefact list to this factory before <see cref="CanCreateWhen"/> or
    /// <see cref="Create"/> is called.
    /// </summary>
    /// <param name="artefacts">The artefacts accumulated so far in the run.</param>
    /// <returns>This factory instance, for chaining.</returns>
    ICliCommandReactionFactory Attach(List<AnonymousArtefact> artefacts);

    /// <summary>
    /// Determines whether this factory can create a reaction for the attached artefacts.
    /// </summary>
    /// <returns><see langword="true"/> if this factory can create a reaction right now; otherwise <see langword="false"/>.</returns>
    bool CanCreateWhen();

    /// <summary>
    /// Creates the reaction.
    /// </summary>
    /// <returns>The created reaction.</returns>
    CliCommandReaction Create();
}
