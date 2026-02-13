namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanCliCommandAnonymousArtefact(CliCommand ranCommand) : AnonymousArtefact(nameof(CliCommand))
{
    public CliCommand RanCommand { get; } = ranCommand;
}