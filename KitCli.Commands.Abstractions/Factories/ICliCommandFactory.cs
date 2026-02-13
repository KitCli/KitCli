using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public interface ICliCommandFactory
{
    bool CanCreateWhen(CliInstruction instruction, List<AnonymousArtefact> artefacts) => true;
    
    CliCommand Create(CliInstruction instruction, List<AnonymousArtefact> artefacts);
}