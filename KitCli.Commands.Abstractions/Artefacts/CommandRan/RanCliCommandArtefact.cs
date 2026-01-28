namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanCliCommandArtefact(CliCommand ranCommand) : CliCommandArtefact(nameof(CliCommand))
{
    public CliCommand RanCommand { get; } = ranCommand;
}