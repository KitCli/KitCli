namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public record RanCliCommandArtefact(CliCommand RanCommand) : Artefact<CliCommand>(nameof(CliCommand), RanCommand);