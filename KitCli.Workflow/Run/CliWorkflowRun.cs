using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
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

    public CliWorkflowRun(
        CliWorkflowRunState state,
        ICliInstructionParser cliInstructionParser,
        ICliInstructionValidator cliInstructionValidator,
        ICliWorkflowCommandProvider workflowCommandProvider,
        ISender sender)
    {
        State = state;
        
        _cliInstructionParser = cliInstructionParser;
        _cliInstructionValidator = cliInstructionValidator;
        _workflowCommandProvider = workflowCommandProvider;
        _sender = sender;
    }

    private bool IsEmptyAsk(string? ask) => !string.IsNullOrEmpty(ask);
    

    public async ValueTask<CliCommandOutcome[]> RespondToAsk(string? ask)
    {
        if (!IsEmptyAsk(ask))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingCliCommandOutcome()];
        }
        
        var instruction = _cliInstructionParser.Parse(ask!);
        
        if (_cliInstructionValidator.IsValidInstruction(instruction))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Running, instruction);
        }
        else
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingCliCommandOutcome()];
        }

        try
        {
            var command = GetCommandFromInstruction(instruction);

            var outcomes = await _sender.Send(command);

            var ranOutcome = new RanCliCommandOutcome(command);
            
            CliCommandOutcome[] allOutcomes = [ranOutcome, ..outcomes];
            
            UpdateStateAfterOutcome(allOutcomes);
            
            return allOutcomes;
        }
        catch (NoCommandGeneratorException)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
            return [new NothingCliCommandOutcome()];
        }
        catch (Exception exception)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Exceptional);
            return [new ExceptionCliCommandOutcome(exception)];
        }
        finally
        {
            UpdateStateWhenFinished();
        }
    }
    
    private CliCommand GetCommandFromInstruction(CliInstruction instruction)
    {
        var priorOutcomes = State
            .AllOutcomeStateChanges()
            // TODO: Write unit test covering this flattening.
            .SelectMany(outcomeChange => outcomeChange.Outcomes)
            .ToList();
        
        return _workflowCommandProvider.GetCommand(instruction, priorOutcomes);
    }

    private void UpdateStateAfterOutcome(CliCommandOutcome[] outcomes)
    {
        var reusableOutcome = outcomes.LastOrDefault(x => x.IsReusable);
        
        var nextState = reusableOutcome != null
            ? ClIWorkflowRunStateStatus.ReachedReusableOutcome
            : ClIWorkflowRunStateStatus.ReachedFinalOutcome;

        State.ChangeTo(nextState, outcomes);
    }

    private void UpdateStateWhenFinished()
    {
        if (!State.WasChangedTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome))
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Finished);
        }
    }
}