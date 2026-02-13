using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public class BasicCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public override bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts) => true;

    public override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts) => new TCliCommand();
}