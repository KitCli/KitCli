using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public interface ICliCommandFactory
{
    ICliCommandFactory Attach(Instruction instruction, List<AnonymousArtefact> artefacts);
    
    bool CanCreateWhen();
    
    CliCommand Create();
}