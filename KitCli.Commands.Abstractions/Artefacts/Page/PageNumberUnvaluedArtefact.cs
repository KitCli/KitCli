namespace KitCli.Commands.Abstractions.Artefacts.Page;

public record PageNumberArtefact(int PageNumber) : Artefact<int>(nameof(PageNumber), PageNumber);