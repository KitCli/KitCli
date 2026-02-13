using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Commands.Factories;

public abstract class ListCliCommandFactory
{
    protected static (int? pageSize, int? pageNumber) GetPaging(CliInstruction instruction, List<AnonymousArtefact> artefacts)
    {
        var pageSizeArtefact = artefacts
            .OfType<int>(ListCliCommand.ArtefactNames.PageSize);

        var pageNumberArtefact = artefacts
            .OfType<int>(ListCliCommand.ArtefactNames.PageNumber);

        var pageSizeArgument = instruction
            .Arguments
            .OfType<int>(ListCliCommand.ArgumentNames.PageSize);

        var pageNumberArgument = instruction
            .Arguments
            .OfType<int>(ListCliCommand.ArgumentNames.PageNumber);

        var pageSize = pageSizeArgument?.ArgumentValue ?? pageSizeArtefact?.Value;
        var pageNumber = pageNumberArgument?.ArgumentValue ?? pageNumberArtefact?.Value;

        return (pageSize, pageNumber);
    }
}