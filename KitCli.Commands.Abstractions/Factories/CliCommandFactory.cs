using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class CliCommandFactory<TCliCommand> : IUnidentifiedCliCommandFactory where TCliCommand : CliCommand
{
    public abstract bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts);

    public abstract CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts);
}