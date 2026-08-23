namespace KitCli.Commands.Abstractions.Artefacts.Page;

/// <summary>
/// The queryable artefact form of a remembered page size.
/// </summary>
/// <param name="PageSize">The remembered page size.</param>
public record PageSizeArtefact(int PageSize) : Artefact<int>(nameof(PageSize), PageSize);