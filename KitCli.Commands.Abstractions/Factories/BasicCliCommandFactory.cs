using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public class BasicCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public sealed override bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts) => true;

    public sealed override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts) => new TCliCommand();
}