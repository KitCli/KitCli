namespace KitCli.Commands.Abstractions.Artefacts;

/// <summary>
/// Defines an outcome of prior commands that can be used to build another.
/// For example, an artefact could be a page number, page size,
/// or an aggregator that can be used to build a query in a subsequent command.
/// </summary>
public abstract record AnonymousArtefact(string Name);