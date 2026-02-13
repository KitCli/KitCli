namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberUnvaluedArtefact(int pageNumber) : Artefact<int>(nameof(pageNumber), pageNumber)
{
    public int PageNumber { get; } = pageNumber;
}