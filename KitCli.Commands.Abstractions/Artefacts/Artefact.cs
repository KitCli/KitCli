namespace KitCli.Commands.Abstractions.Artefacts;

public record Artefact<TArtefactValue>(string Name, TArtefactValue Value) : AnonymousArtefact(Name);