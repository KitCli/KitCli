using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Instructions.Abstractions;

public class TestCliCommandFactory : ICliCommandFactory<TestCliCommand>
{
    public CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
        => new TestCliCommand();
}