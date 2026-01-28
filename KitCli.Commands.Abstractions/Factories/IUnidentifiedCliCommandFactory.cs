using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public interface IUnidentifiedCliCommandFactory
{
    bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts) => true;
    
    CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts);
}