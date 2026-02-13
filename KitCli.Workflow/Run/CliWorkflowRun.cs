using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Outcomes.Skippable;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run.State;
using MediatR;

namespace KitCli.Workflow.Run;

public class CliWorkflowRun : ICliWorkflowRun
{
    public ICliWorkflowRunState State { get; }
    
    private readonly ICliInstructionParser _cliInstructionParser;
    private readonly ICliInstructionValidator _cliInstructionValidator;
    private readonly ICliWorkflowCommandProvider _workflowCommandProvider;

    private readonly ISender _sender;
    private readonly IPublisher _publisher;

    public CliWorkflowRun(
        CliWorkflowRunState state,
        ICliInstructionParser cliInstructionParser,
        ICliInstructionValidator cliInstructionValidator,
        ICliWorkflowCommandProvider workflowCommandProvider,
        ISender sender,
        IPublisher publisher)
    {
        State = state;
        
        _cliInstructionParser = cliInstructionParser;
        _cliInstructionValidator = cliInstructionValidator;
        _workflowCommandProvider = workflowCommandProvider;
        _sender = sender;
        _publisher = publisher;
    }

    public async ValueTask<Outcome[]> RespondToAsk(string? ask)
    {
        if (!IsValidAsk(ask))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingOutcome()];
        }
        
        var instruction = _cliInstructionParser.Parse(ask!);
        
        if (_cliInstructionValidator.IsValidInstruction(instruction))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction);
        }
        else
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingOutcome()];
        }
        
        var priorOutcomes = AllPriorOutcomes();
        
        var command = _workflowCommandProvider.GetCommand(instruction, priorOutcomes);
        
        return await ExecuteCommand(command);
    }

    public async ValueTask<Outcome[]> RespondToNext()
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
            var outcomes = await _sender.Send(command);
            
            Outcome[] allOutcomes = [new RanOutcome(command), ..outcomes];
            
            await TriggerCommandReactions(allOutcomes);
            UpdateStateAfterOutcome(allOutcomes);
            
            return allOutcomes;
        }
        catch (NoCommandGeneratorException)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingOutcome()];
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
        var reusableOutcome = outcomes.LastOrDefault(x => x.IsReusable);
        
        if (reusableOutcome is null)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome, outcomes);
            return;
        }
        
        if (reusableOutcome is NextCliCommandOutcome)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.MovePastAsk, outcomes);
            return;
        }
        
        State.ChangeTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome, outcomes);
    }

    private void UpdateStateWhenFinished()
    {
        var runOngoing = State.WasChangedTo(
            ClIWorkflowRunStateStatus.MovePastAsk,
            ClIWorkflowRunStateStatus.ReachedReusableOutcome);
        
        if (!runOngoing)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Finished);
        }
    }
    
    // TODO: Perhaps move to extension somewhere
    private List<Outcome> AllPriorOutcomes()
        => State
            .AllOutcomeStateChanges()
            .SelectMany(change => change.Outcomes)
            .ToList();
}