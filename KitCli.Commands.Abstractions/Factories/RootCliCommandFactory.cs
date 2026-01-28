using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class RootCliCommandFactory
{
    public bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts) => false;
}