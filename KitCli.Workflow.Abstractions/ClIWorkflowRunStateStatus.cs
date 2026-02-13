namespace KitCli.Workflow.Abstractions;

public enum ClIWorkflowRunStateStatus
{
    Created,
    Running,
    InvalidAsk,
    Exceptional,
    MovePastAsk,
    InvalidMovePastAsk,
    ReachedReusableOutcome,
    ReachedFinalOutcome,
    Finished,
}