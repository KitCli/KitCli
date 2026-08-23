using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Factories;

/// <summary>
/// Base factory for commands that support paging, providing a shared way to resolve the
/// page size and page number from arguments or artefacts.
/// </summary>
/// <typeparam name="TCliCommand">The type of command this factory builds.</typeparam>
public abstract class PagedCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    // TODO: It feels like this breaks the 'GetArtefact' and 'GetArgument' methods, but it is a common pattern that I don't want to repeat in every factory.
    /// <summary>
    /// Resolves the page size and page number to use, preferring values supplied as command-line
    /// arguments, falling back to values carried in artefacts, and defaulting to a page size of 20
    /// and page number of 1 if neither is present.
    /// </summary>
    /// <returns>The resolved page size and page number.</returns>
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