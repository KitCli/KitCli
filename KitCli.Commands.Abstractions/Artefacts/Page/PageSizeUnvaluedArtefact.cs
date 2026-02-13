namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeUnvaluedArtefact(int pageSize) 
    : Artefact<int>(nameof(pageSize), pageSize)
{
    public int PageSize { get; } = pageSize;
}