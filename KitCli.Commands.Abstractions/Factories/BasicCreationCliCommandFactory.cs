using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class BasicCreationCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand
{
    public sealed override bool CanCreateWhen(CliInstruction instruction, List<AnonymousArtefact> artefacts) => true;
}