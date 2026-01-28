namespace KitCli.Workflow.Abstractions;

public enum ClIWorkflowRunStateStatus
{
    Created,
    Running,
    InvalidAsk,
    Exceptional,
    ReachedReusableOutcome,
    ReachedFinalOutcome,
    Finished,
}