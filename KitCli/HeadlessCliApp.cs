using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// A KitCli app with nothing attached to its input (<c>myapp /command --flag value</c>). The process
/// args, joined, are the only ask it will ever get, so the session is however far one run gets without
/// being asked anything else — a chained command runs every step, but once that run is over there is
/// nothing to start another with.
/// </summary>
public abstract class HeadlessCliApp : CliApp
{
    /// <summary>
    /// Initializes the shared workflow and I/O references used by the run.
    /// </summary>
    /// <param name="workflow">The workflow driving this app's run.</param>
    /// <param name="io">The I/O implementation used to write output and observe cancellation.</param>
    protected HeadlessCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

    /// <summary>
    /// Runs <paramref name="args"/>, joined into one ask, to the end, then stops the workflow.
    /// </summary>
    /// <param name="outcomeIoWriters">The writers used to render the run's outcomes.</param>
    /// <param name="args">The process args to join into the ask driving this run.</param>
    /// <returns>A task that completes once the run has finished and the session has ended.</returns>
    public override async Task Run(List<IOutcomeIoWriter> outcomeIoWriters, string[]? args = null)
    {
        OnSessionStart();

        Io.Pause();

        SetUpEventHandlers();

        await ExecuteRunOperation(string.Join(" ", args ?? []), outcomeIoWriters);

        Workflow.Stop();

        OnSessionEnd(Workflow.Runs);
    }
}
