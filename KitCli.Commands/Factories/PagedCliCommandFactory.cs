using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Factories;

public abstract class PagedCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    // TODO: It feels like this breaks the 'GetArtefact' and 'GetArgument' methods, but it is a common pattern that I don't want to repeat in every factory.
    protected (int pageSize, int pageNumber) GetPaging()
    {
        var pageSizeArtefact = GetArtefact<int>(nameof(PageSizeArtefact.PageSize));
        var pageNumberArtefact = GetArtefact<int>(nameof(PageNumberArtefact.PageNumber));
        
        var pageSizeArgument = GetArgument<int>(PagedCliCommand<object, object>.ArgumentNames.PageSize);
        var pageNumberArgument = GetArgument<int>(PagedCliCommand<object, object>.ArgumentNames.PageNumber);

        var pageSize = pageSizeArgument?.Value ?? pageSizeArtefact?.Value ?? 20;
        var pageNumber = pageNumberArgument?.Value ?? pageNumberArtefact?.Value ?? 1;
        
        return (pageSize, pageNumber);
    }
}