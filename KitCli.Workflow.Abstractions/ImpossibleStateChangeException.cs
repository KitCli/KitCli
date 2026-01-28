namespace KitCli.Workflow.Abstractions;

public class ImpossibleStateChangeException(string message)
    : CliWorkflowException(CliWorkflowExceptionCode.ImpossibleStateChange, message)
{
}