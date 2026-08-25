namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// The next command, provided by the handler that chose it. Nothing else builds it, so its
/// <c>ICliCommandFactory</c> never runs and never sees the run's artefacts — a handler picks this by
/// calling <c>ByMovingToCommand(command)</c>, which suits a command that takes its data by constructor.
/// </summary>
/// <param name="ProvidedCommand">The command to move to.</param>
public record ProvidedNextCliCommandOutcome(CliCommand ProvidedCommand) : NextCliCommandOutcome();
