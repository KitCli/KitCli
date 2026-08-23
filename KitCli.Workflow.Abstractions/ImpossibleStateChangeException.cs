namespace KitCli.Workflow.Abstractions;

/// <summary>
/// Thrown when a <see cref="ICliWorkflowRunState"/> is asked to change to a status that isn't
/// listed as a legal transition from its current status.
/// </summary>
/// <param name="message">A human-readable description of the rejected transition.</param>
public class ImpossibleStateChangeException(string message)
    : CliWorkflowException(CliWorkflowExceptionCode.ImpossibleStateChange, message)
{
}