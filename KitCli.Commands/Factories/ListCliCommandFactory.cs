using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Factories;

public abstract class ListCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    protected (int? pageSize, int? pageNumber) GetPaging()
    {
        var pageSizeArtefact = GetArtefact<int>(ListCliCommand.ArtefactNames.PageSize);
        var pageNumberArtefact = GetArtefact<int>(ListCliCommand.ArtefactNames.PageNumber);
        
        var pageSizeArgument = GetArgument<int>(ListCliCommand.ArgumentNames.PageSize);
        var pageNumberArgument = GetArgument<int>(ListCliCommand.ArgumentNames.PageNumber);
        
        var pageSize = pageSizeArgument?.ArgumentValue ?? pageSizeArtefact?.Value;
        var pageNumber = pageNumberArgument?.ArgumentValue ?? pageNumberArtefact?.Value;
        
        return (pageSize, pageNumber);
    }
}