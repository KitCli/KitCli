using KitCli.Commands.Abstractions.Artefacts;

namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// A custom factory for creating a specific <see cref="CliCommandReaction"/>.
/// This is useful when the reaction creation logic is complex and cannot be easily handled by a simple constructor or when it requires access to the artefacts for decision-making.
/// If you don't need to use Artefacts, you do not need to create this factory: basic reactions are automatically created.
/// </summary>
/// <typeparam name="TReaction">A custom created Reaction.</typeparam>
public abstract class CliCommandReactionFactory<TReaction> : ICliCommandReactionFactory where TReaction : CliCommandReaction
{
    /// <summary>
    /// The artefacts currently attached to this factory, or an empty list if none have been attached.
    /// </summary>
    protected List<AnonymousArtefact> Artefacts => _artefacts ?? [];

    private List<AnonymousArtefact>? _artefacts;

    /// <inheritdoc/>
    public abstract bool CanCreateWhen();

    /// <inheritdoc/>
    public abstract CliCommandReaction Create();

    /// <inheritdoc/>
    public ICliCommandReactionFactory Attach(List<AnonymousArtefact> artefacts)
    {
        _artefacts = artefacts;

        return this;
    }

    /// <summary>
    /// Determines whether the attached artefacts contain any artefact of the given type, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns><see langword="true"/> if a matching artefact exists; otherwise <see langword="false"/>.</returns>
    protected bool AnyArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();

        return artefactName == null
            ? typedArtefacts.Any()
            : typedArtefacts.Any(artefact => artefact.Name == artefactName);
    }

    /// <summary>
    /// Gets the last artefact of the given type from the attached artefacts, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching artefact, or <see langword="null"/> if none match.</returns>
    protected Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var typedArtefacts = GetArtefacts<TArtefactType>();

        return artefactName == null
            ? typedArtefacts.LastOrDefault()
            : typedArtefacts.LastOrDefault(artefact => artefact.Name == artefactName);
    }

    /// <summary>
    /// Gets the last artefact of the given type from the attached artefacts, optionally filtered by name.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <param name="artefactName">The artefact name to filter by, or <see langword="null"/> to match any name.</param>
    /// <returns>The last matching artefact.</returns>
    /// <exception cref="Exception">Thrown when no matching artefact is found.</exception>
    protected Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
    {
        var artefact = GetArtefact<TArtefactType>(artefactName);

        if (artefact == null)
        {
            // TODO: Handle further upstream in future.
            throw new Exception($"Required artefact '{artefactName}' of type '{typeof(TArtefactType).Name}' not found.");
        }

        return artefact;
    }

    /// <summary>
    /// Gets every artefact of the given type from the attached artefacts.
    /// </summary>
    /// <typeparam name="TArtefactType">The artefact value type to look for.</typeparam>
    /// <returns>The matching artefacts, in the run's history order.</returns>
    /// <exception cref="Exception">Thrown when no artefacts have been attached via <see cref="Attach"/>.</exception>
    protected IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>() where TArtefactType : notnull
    {
        ValidateAttached();

        return _artefacts!.OfType<Artefact<TArtefactType>>();
    }

    private void ValidateAttached()
    {
        if (_artefacts == null)
        {
            throw new Exception("Factory not registered, automatic attaching.");
        }
    }
}
