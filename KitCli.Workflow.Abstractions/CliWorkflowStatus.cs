namespace KitCli.Workflow.Abstractions;

/// <summary>
/// The lifecycle status of an <see cref="ICliWorkflow"/> as a whole, as opposed to any single
/// <see cref="ICliWorkflowRun"/>.
/// </summary>
public enum CliWorkflowStatus
{
    /// <summary>The workflow is accepting asks and dispatching runs.</summary>
    Started,

    /// <summary>The workflow has been stopped, typically by a command such as exit.</summary>
    Stopped
}