namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// The next command, specified by type rather than built. Nothing is constructed when the handler names
/// it: whoever runs it resolves <see cref="SpecifiedCommandType"/> through the same factory path any
/// instruction takes, so the factory sees the run's accumulated artefacts. A handler picks this by
/// calling <c>ByMovingToCommand&lt;TCommand&gt;()</c>.
/// </summary>
/// <param name="SpecifiedCommandType">The type of the command to move to.</param>
public record SpecifiedNextCliCommandOutcome(Type SpecifiedCommandType) : NextCliCommandOutcome();
