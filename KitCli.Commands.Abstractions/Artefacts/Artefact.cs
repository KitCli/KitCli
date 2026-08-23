namespace KitCli.Commands.Abstractions.Artefacts;

/// <summary>
/// A named, typed artefact carrying the value later commands can query it for.
/// </summary>
/// <typeparam name="TArtefactValue">The type of the value carried by this artefact.</typeparam>
/// <param name="Name">The name this artefact is queryable by, in addition to its type.</param>
/// <param name="Value">The value carried by this artefact.</param>
public abstract record Artefact<TArtefactValue>(string Name, TArtefactValue Value) : AnonymousArtefact(Name);