namespace KitCli.Commands.Abstractions.Outcomes;

/// <summary>
/// What a command handler produced and how it affects the workflow run. Every <see cref="OutcomeList"/>
/// <c>By...</c> method appends one of these; the workflow engine inspects the last outcome in the
/// returned array to decide whether the run continues, pauses, or ends.
/// </summary>
/// <param name="Kind">Whether this outcome has no effect on the run, is queryable by later commands, or ends the run.</param>
public abstract record Outcome(OutcomeKind Kind)
{
    /// <summary>
    /// Whether this outcome's <see cref="Kind"/> is <see cref="OutcomeKind.Reusable"/>.
    /// </summary>
    public bool IsReusable => Kind == OutcomeKind.Reusable;

    /// <summary>
    /// How long this outcome stays readable. <see cref="OutcomeLifetime.Run"/> unless the outcome's
    /// constructor passes another.
    /// </summary>
    public OutcomeLifetime Lifetime { get; protected init; } = OutcomeLifetime.Run;

    /// <summary>
    /// Creates an outcome that stays readable for the given lifetime.
    /// </summary>
    /// <param name="lifetime">How long this outcome stays readable.</param>
    /// <param name="kind">Whether this outcome has no effect on the run, is queryable by later commands, or ends the run.</param>
    protected Outcome(OutcomeLifetime lifetime, OutcomeKind kind) : this(kind)
        => Lifetime = lifetime;
}