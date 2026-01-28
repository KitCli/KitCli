namespace KitCli.Commands.Abstractions.Artefacts.Page;

public class PageSizeCliCommandArtefact(int pageSize) 
    : ValuedCliCommandArtefact<int>(nameof(pageSize), pageSize)
{
    public int PageSize { get; } = pageSize;
}