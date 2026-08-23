namespace KitCli.Workflow.Abstractions;

/// <summary>
/// Identifies the specific kind of error a <see cref="CliWorkflowException"/> represents.
/// </summary>
public enum CliWorkflowExceptionCode
{
    /// <summary>A run's state was asked to change to a status not listed as legal from its current status.</summary>
    ImpossibleStateChange
}