using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;

namespace KitCli.Workflow.Commands.Exit;

public class ExitCliCommandFactory : ICliCommandFactory<ExitCliCommand>
{
    public CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
        => new ExitCliCommand();
}