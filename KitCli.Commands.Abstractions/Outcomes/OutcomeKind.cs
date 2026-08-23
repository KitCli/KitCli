namespace KitCli.Commands.Abstractions.Outcomes;

/// <summary>
/// The effect an <see cref="Outcome"/> has on the workflow run it belongs to.
/// </summary>
public enum OutcomeKind
{
    /// <summary>
    /// Has no effect on the workflow run.
    /// </summary>
    Anonymous,
    
    /// <summary>
    /// Allows further operation on the same run.
    /// </summary>
    Reusable,
    
    /// <summary>
    /// Ends the workflow run.
    /// </summary>
    Final
}