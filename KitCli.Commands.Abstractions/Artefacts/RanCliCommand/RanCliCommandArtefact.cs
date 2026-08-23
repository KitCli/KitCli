namespace KitCli.Commands.Abstractions.Artefacts.RanCliCommand;

/// <summary>
/// The queryable artefact form marking that a given command ran, so a later command factory can check
/// what the previous command was (see <c>CliCommandFactory{T}.LastCommandWas</c>).
/// </summary>
/// <param name="RanCommand">The command that ran.</param>
public record RanCliCommandArtefact(CliCommand RanCommand)
    : Artefact<CliCommand>(RanCommand.GetType().Name, RanCommand);