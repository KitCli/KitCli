namespace KitCli.Commands.Abstractions.Artefacts.Page;

/// <summary>
/// The queryable artefact form of a remembered page number.
/// </summary>
/// <param name="PageNumber">The remembered page number.</param>
public record PageNumberArtefact(int PageNumber) : Artefact<int>(nameof(PageNumber), PageNumber);