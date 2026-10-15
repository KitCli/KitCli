namespace KitCli.Commands.Abstractions.Outcomes;

/// <summary>
/// How long an <see cref="Outcome"/> stays readable by later commands.
/// </summary>
public enum OutcomeLifetime
{
    /// <summary>
    /// Readable until the run that made it reaches a final outcome.
    /// </summary>
    Run,

    /// <summary>
    /// Readable by every later run, until the app exits.
    /// </summary>
    Session
}
