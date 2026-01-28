namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageNumberCliCommandArtefact(int pageNumber) : ValuedCliCommandArtefact<int>(nameof(pageNumber), pageNumber)
{
    public int PageNumber { get; } = pageNumber;
}