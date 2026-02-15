namespace KitCli.Commands.Abstractions.Artefacts.Page;

public record PageSizeArtefact(int PageSize) : Artefact<int>(nameof(PageSize), PageSize);