using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class CliCommandFactory<TCliCommand> : ICliCommandFactory where TCliCommand : CliCommand
{
    public abstract bool CanCreateWhen(CliInstruction instruction, List<AnonymousArtefact> artefacts);

    public abstract CliCommand Create(CliInstruction instruction, List<AnonymousArtefact> artefacts);
}