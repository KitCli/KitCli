namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// Marks that the given command ran, so a later command can check what the previous command was via
/// its artefact (see <c>CliCommandFactory{T}.LastCommandWas</c>).
/// </summary>
/// <param name="Command">The command that ran.</param>
public record RanCliCommandOutcome(CliCommand Command) : Outcome(OutcomeKind.Reusable);