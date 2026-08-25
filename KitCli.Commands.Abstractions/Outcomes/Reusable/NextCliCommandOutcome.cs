namespace KitCli.Commands.Abstractions.Outcomes.Reusable;

/// <summary>
/// Remembers that another command should run next. Which of the two kinds a handler returns is the
/// handler's choice, made when it calls <c>ByMovingToCommand</c>: see
/// <see cref="ProvidedNextCliCommandOutcome"/> and <see cref="SpecifiedNextCliCommandOutcome"/>.
/// </summary>
public abstract record NextCliCommandOutcome() : Outcome(OutcomeKind.Reusable);
