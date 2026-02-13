using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class BasicDecisionCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts) => new TCliCommand();
}