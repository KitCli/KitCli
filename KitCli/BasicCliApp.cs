using KitCli.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// Default <see cref="CliApp"/> with no additional behavior — the interactive app produced by
/// <c>CliAppBuilder.WithBasicApp()</c> for consumers that don't need to override any lifecycle hooks.
/// </summary>
public class BasicCliApp : CliApp
{
    /// <summary>
    /// Initializes the shared workflow and I/O references used by the interactive loop.
    /// </summary>
    /// <param name="workflow">The workflow driving this app's runs.</param>
    /// <param name="io">The I/O implementation used to ask, write, and observe cancellation.</param>
    public BasicCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }
}
