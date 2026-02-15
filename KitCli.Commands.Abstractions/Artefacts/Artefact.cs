namespace KitCli.Commands.Abstractions.Artefacts;

public abstract record Artefact<TArtefactValue>(string Name, TArtefactValue Value) : AnonymousArtefact(Name);