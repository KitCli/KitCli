using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow.Run;

/// <summary>
/// Default <see cref="ICliWorkflowRun"/> implementation: drives one execution arc from an ask
/// through to a final outcome by parsing/validating input, resolving and executing commands via
/// MediatR, publishing reaction outcomes, and enforcing the run's state machine transitions.
/// </summary>
public class CliWorkflowRun : ICliWorkflowRun
{
    /// <inheritdoc/>
    public ICliWorkflowRunState State { get; }

    private readonly IServiceScope _serviceScope;
    private readonly IInstructionParser _instructionParser;
    private readonly IInstructionValidator _instructionValidator;
    private readonly ICliWorkflowCommandProvider _workflowCommandProvider;

    private readonly ISender _sender;
    private readonly IPublisher _publisher;
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Creates a run bound to a specific DI scope and the services resolved from it. The scope is
    /// disposed once the run finishes.
    /// </summary>
    /// <param name="state">The run's state machine, initially at its default (not-yet-<c>Created</c>) status.</param>
    /// <param name="serviceScope">The DI scope this run owns; disposed when the run reaches <c>Finished</c>.</param>
    /// <param name="instructionParser">Parses raw ask strings into instructions.</param>
    /// <param name="instructionValidator">Validates a parsed instruction before a command is resolved for it.</param>
    /// <param name="workflowCommandProvider">Resolves the command to execute for a valid instruction.</param>
    /// <param name="sender">Dispatches resolved commands to their MediatR handlers.</param>
    /// <param name="publisher">Publishes reaction outcomes raised while executing a command.</param>
    /// <param name="cancellationToken">
    /// The token passed through to <see cref="ISender.Send"/> for every command this run executes.
    /// </param>
    public CliWorkflowRun(
        CliWorkflowRunState state,
        IServiceScope serviceScope,
        IInstructionParser instructionParser,
        IInstructionValidator instructionValidator,
        ICliWorkflowCommandProvider workflowCommandProvider,
        ISender sender,
        IPublisher publisher,
        CancellationToken cancellationToken = default)
    {
        State = state;

        _serviceScope = serviceScope;
        _instructionParser = instructionParser;
        _instructionValidator = instructionValidator;
        _workflowCommandProvider = workflowCommandProvider;
        _sender = sender;
        _publisher = publisher;
        _cancellationToken = cancellationToken;
    }

    /// <inheritdoc/>
    public async ValueTask<Outcome[]> RespondToAsk(string? ask)
    {
        if (!IsValidAsk(ask))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingOutcome()];
        }
        
        var instruction = _instructionParser.Parse(ask!);

        if (!_instructionValidator.IsValid(instruction))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingOutcome()];
        }
        
        var priorOutcomes = AllPriorOutcomes();

        CliCommand command;
        try
        {
            command = _workflowCommandProvider.GetCommand(instruction, priorOutcomes);
        }
        catch (NoCommandGeneratorException)
        {
            if (!State.WasChangedTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome))
            {
                State.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction);
                State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
                UpdateStateWhenFinished();
            }

            return [new NothingOutcome()];
        }

        State.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction);
        return await ExecuteCommand(command);
    }

    /// <inheritdoc/>
    public async ValueTask<Outcome[]> MoveToNext()
    {
        if (!IsValidMovePastAsk())
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidMovePastAsk);
            return [new NothingOutcome()];
        }

        State.ChangeTo(ClIWorkflowRunStateStatus.Running);

        var nextOutcome = AllPriorOutcomes()
            .OfType<NextCliCommandOutcome>()
            .Last();

        return await ExecuteCommand(nextOutcome.NextCommand);
    }

    private async Task<Outcome[]> ExecuteCommand(CliCommand command)
    {
        try
        {
            var outcomes = await _sender.Send(command, _cancellationToken);
            
            Outcome[] allOutcomes = [new RanCliCommandOutcome(command), ..outcomes];
            
            await TriggerCommandReactions(allOutcomes);
            UpdateStateAfterOutcome(allOutcomes);
            
            return allOutcomes;
        }
        catch (Exception exception)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Exceptional);
            return [new ExceptionOutcome(exception)];
        }
        finally
        {
            UpdateStateWhenFinished();
        }
    }
    
    // TODO: Could probably be put in line.
    private bool IsValidAsk(string? ask)
        => !string.IsNullOrEmpty(ask);

    // TODO: Could probably be moved to an extension method.
    private bool IsValidMovePastAsk() 
        => AllPriorOutcomes()
            .OfType<NextCliCommandOutcome>()
            .Any();

    private Task TriggerCommandReactions(Outcome[] outcomes)
    {
        var publishTasks = outcomes
            .OfType<ReactionOutcome>()
            .Select(outcome => _publisher.Publish(outcome.Reaction))
            .ToList();

        return Task.WhenAll(publishTasks);
    }

    private void UpdateStateAfterOutcome(Outcome[] outcomes)
    {
        var lastOutcome = outcomes.LastOrDefault();
        
        if (lastOutcome is null || !lastOutcome.IsReusable)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, outcomes);
            return;
        }
        
        if (lastOutcome is NextCliCommandOutcome)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.MovePastAsk, outcomes);
            return;
        }
        
        State.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome, outcomes);
    }

    private void UpdateStateWhenFinished()
    {
        var runComplete = State.WasChangedTo(
            ClIWorkflowRunStateStatus.ReachedFinalOutcome,
            ClIWorkflowRunStateStatus.InvalidAsk,
            ClIWorkflowRunStateStatus.Exceptional);

        if (runComplete)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Finished);
            _serviceScope.Dispose();
        }
    }

    // TODO: Perhaps move to extension somewhere
    private List<Outcome> AllPriorOutcomes()
        => State
            .AllOutcomeStateChanges()
            .SelectMany(change => change.Outcomes)
            .ToList();
}