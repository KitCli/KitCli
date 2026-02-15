namespace KitCli.Commands.Abstractions.Artefacts.RanCliCommand;

public record RanCliCommandArtefact(CliCommand RanCommand)
    : Artefact<CliCommand>(RanCommand.GetType().Name, RanCommand);