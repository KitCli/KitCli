using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Factories;

public abstract class PagedCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    protected (int pageSize, int pageNumber) GetPaging()
    {
        var pageSizeArtefact = GetArtefact<int>(PagedCliCommand<object, object>.ArtefactNames.PageSize);
        var pageNumberArtefact = GetArtefact<int>(PagedCliCommand<object, object>.ArtefactNames.PageNumber);
        
        var pageSizeArgument = GetArgument<int>(PagedCliCommand<object, object>.ArgumentNames.PageSize);
        var pageNumberArgument = GetArgument<int>(PagedCliCommand<object, object>.ArgumentNames.PageNumber);

        var pageSize = pageSizeArgument?.Value ?? pageSizeArtefact?.Value ?? 20;
        var pageNumber = pageNumberArgument?.Value ?? pageNumberArtefact?.Value ?? 1;
        
        return (pageSize, pageNumber);
    }
}