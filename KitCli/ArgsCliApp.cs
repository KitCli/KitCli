using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Workflow.Abstractions;

namespace KitCli;

/// <summary>
/// Entry point for a non-interactive, one-shot KitCli invocation (<c>myapp /command --flag value</c>).
/// Joins <c>args</c> into a single ask, feeds it through the same <see cref="ICliWorkflowRun.RespondToAsk"/>
/// pipeline an interactive ask would use, writes the resulting outcomes, then stops the workflow — exactly
/// one command runs per invocation, regardless of what state that run reaches.
/// </summary>
public abstract class ArgsCliApp : CliApp
{
    /// <summary>
    /// Initializes the shared workflow and I/O references used by the one-shot run.
    /// </summary>
    /// <param name="workflow">The workflow driving this app's run.</param>
    /// <param name="io">The I/O implementation used to write output and observe cancellation.</param>
    protected ArgsCliApp(ICliWorkflow workflow, ICliIo io) : base(workflow, io)
    {
    }

    /// <summary>
    /// Runs the one-shot host loop: fires <see cref="CliApp.OnSessionStart"/>, wires cancellation,
    /// joins <paramref name="args"/> into a single ask, responds to it, writes the resulting outcomes,
    /// then calls <see cref="ICliWorkflow.Stop"/> and fires <see cref="CliApp.OnSessionEnd"/>.
    /// </summary>
    /// <param name="outcomeIoWriters">The writers used to render the run's outcomes.</param>
    /// <param name="args">The process args to join into the ask driving this run.</param>
    /// <returns>A task that completes once the single run has finished and the workflow has stopped.</returns>
    public async Task Run(List<IOutcomeIoWriter> outcomeIoWriters, string[] args)
    {
        OnSessionStart();
        
        Io.Pause();
        
        SetUpEventHandlers();
        
        var run = Workflow.NextRun();
            
        OnRunCreated(run);
            
        var ask = string.Join(" ",  args);
        
        var runTask =  run.RespondToAsk(ask);

        OnRunStarted(run, ask);
        
        var outcomes = await runTask;
            
        WriteOutcomes(outcomes, outcomeIoWriters);
            
        OnRunComplete(run, outcomes);
            
        Workflow.Stop();
        
        OnSessionEnd(Workflow.Runs);
    }
}