using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run;
using KitCli.Workflow.Run.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow;

/// <summary>
/// State machine of a command line interface.
/// </summary>
public class CliWorkflow(IServiceProvider serviceProvider) : ICliWorkflow
{
    public List<ICliWorkflowRun> Runs { get; } = [];
    public CliWorkflowStatus Status { get; set; } = CliWorkflowStatus.Started;


    /// <summary>
    /// Create a new run, a sub-state machine of an individual execution.
    /// </summary>
    /// <returns>A sub-state mchine of an individual execution.</returns>
    public ICliWorkflowRun NextRun()
    {
        var lastRunNotHavingReachedFinalOutcome = Runs
            .SingleOrDefault(run => !run.State.WasChangedTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome));

        return lastRunNotHavingReachedFinalOutcome ?? CreateNewRun();
    }

    /// <summary>
    /// Close the state machine.
    /// </summary>
    public void Stop()
    {
        Status = CliWorkflowStatus.Stopped;
    }
    
    private ICliWorkflowRun CreateNewRun()
    {
        var state = new CliWorkflowRunState();
        
        var instructionParser = serviceProvider.GetRequiredService<ICliInstructionParser>();

        var instructionValidator = serviceProvider.GetRequiredService<ICliInstructionValidator>();
        
        var commandProvider = serviceProvider.GetRequiredService<ICliWorkflowCommandProvider>();
        
        var sender = serviceProvider.GetRequiredService<ISender>();
        
        var publisher = serviceProvider.GetRequiredService<IPublisher>();
        
        var run = new CliWorkflowRun(
            state,
            instructionParser,
            instructionValidator,
            commandProvider,
            sender,
            publisher);
        
        Runs.Add(run);

        return run;
    }
}