namespace KitCli.Commands.Abstractions.Outcomes;

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