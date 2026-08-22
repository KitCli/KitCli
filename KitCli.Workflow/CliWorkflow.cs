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
public class CliWorkflow(IServiceScopeFactory serviceScopeFactory) : ICliWorkflow
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public List<ICliWorkflowRun> Runs { get; } = [];
    public CliWorkflowStatus Status { get; set; } = CliWorkflowStatus.Started;
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;


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

    public void InterruptCurrentRun()
    {
        _cancellationTokenSource.Cancel();
        Stop();
    }

    private ICliWorkflowRun CreateNewRun()
    {
        var state = new CliWorkflowRunState();

        var serviceScope = serviceScopeFactory.CreateScope();

        var instructionParser = serviceScope
            .ServiceProvider
            .GetRequiredService<IInstructionParser>();

        var instructionValidator = serviceScope
            .ServiceProvider
            .GetRequiredService<IInstructionValidator>();

        var commandProvider = serviceScope
            .ServiceProvider
            .GetRequiredService<ICliWorkflowCommandProvider>();

        var sender = serviceScope
            .ServiceProvider
            .GetRequiredService<ISender>();

        var publisher = serviceScope
            .ServiceProvider
            .GetRequiredService<IPublisher>();

        var run = new CliWorkflowRun(
            state,
            serviceScope,
            instructionParser,
            instructionValidator,
            commandProvider,
            sender,
            publisher,
            CancellationToken);
        
        Runs.Add(run);

        return run;
    }
}