using KitCli.Abstractions.Exceptions;

namespace KitCli.Workflow.Abstractions;

/// <summary>
/// Base exception type for errors raised by the workflow run state machine.
/// </summary>
/// <param name="code">The specific kind of workflow error this exception represents.</param>
/// <param name="message">A human-readable description of the error.</param>
public class CliWorkflowException(CliWorkflowExceptionCode code, string message)
    : CliException(CliExceptionCode.Command, message)
{
    /// <summary>The specific kind of workflow error this exception represents.</summary>
    public new CliWorkflowExceptionCode Code = code;
}