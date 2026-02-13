using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

public abstract class BasicDecisionCliCommandFactory<TCliCommand> : CliCommandFactory<TCliCommand> where TCliCommand : CliCommand, new()
{
    public sealed override CliCommand Create(CliInstruction instruction, List<AnonymousArtefact> artefacts) => new TCliCommand();
}