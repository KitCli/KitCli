using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Abstractions.Validators;
using KitCli.Instructions.Parsers;
using KitCli.Workflow.Abstractions;
using KitCli.Workflow.Commands;
using KitCli.Workflow.Run.State;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    private readonly ICliWorkflowReactionProvider _workflowReactionProvider;
    private readonly InstructionSettings _instructionSettings;

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
    /// <param name="workflowReactionProvider">Resolves the reaction to publish for a specified reaction type.</param>
    /// <param name="instructionSettings">
    /// The configured instruction settings, used to prefix any suggested next-command names.
    /// </param>
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
        ICliWorkflowReactionProvider workflowReactionProvider,
        IOptions<InstructionSettings> instructionSettings,
        ISender sender,
        IPublisher publisher,
        CancellationToken cancellationToken = default)
    {
        State = state;

        _serviceScope = serviceScope;
        _instructionParser = instructionParser;
        _instructionValidator = instructionValidator;
        _workflowCommandProvider = workflowCommandProvider;
        _workflowReactionProvider = workflowReactionProvider;
        _instructionSettings = instructionSettings.Value;
        _sender = sender;
        _publisher = publisher;
        _cancellationToken = cancellationToken;
    }

    /// <inheritdoc/>
    public async ValueTask<Outcome[]> RespondToAsk(string? ask)
    {
        if (!IsValidAsk(ask))
        {
            return RejectAsk();
        }

        var instruction = _instructionParser.Parse(ask!);

        if (!_instructionValidator.IsValid(instruction))
        {
            return RejectAsk();
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

                return [new NothingOutcome()];
            }

            return SuggestNextCommands(priorOutcomes);
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

            UpdateStateWhenFinished();

            return [new NothingOutcome()];
        }

        State.ChangeTo(ClIWorkflowRunStateStatus.Running);

        var nextOutcome = AllPriorOutcomes()
            .OfType<NextCliCommandOutcome>()
            .Last();

        CliCommand command;
        try
        {
            command = GetNextCommandToMoveTo(nextOutcome);
        }
        catch (Exception exception)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Exceptional);
            UpdateStateWhenFinished();

            return [new ExceptionOutcome(exception)];
        }

        return await ExecuteCommand(command);
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

    /// <summary>
    /// The command to run next. A provided outcome already carries one; a specified outcome names a type,
    /// which the run puts in a fresh instruction and resolves like any other, so its factory sees the
    /// run's artefacts.
    /// </summary>
    private CliCommand GetNextCommandToMoveTo(NextCliCommandOutcome nextOutcome)
    {
        if (nextOutcome is ProvidedNextCliCommandOutcome providedNextCliCommandOutcome)
        {
            return providedNextCliCommandOutcome.ProvidedCommand;
        }

        if (nextOutcome is SpecifiedNextCliCommandOutcome specifiedNextCliCommandOutcome)
        {
            var instructionName = CliCommand.GetInstructionName(specifiedNextCliCommandOutcome.SpecifiedCommandType);
            var instruction = Instruction.Empty with
            {
                Prefix = _instructionSettings.Prefix.ToString(),
                Name = instructionName,
                Arguments = [..specifiedNextCliCommandOutcome.Arguments
                    .Select(argument => argument.ToInstructionArgument())]
            };

            var allPriorOutcomes = AllPriorOutcomes();

            return _workflowCommandProvider.GetCommand(instruction, allPriorOutcomes);
        }

        throw new NotSupportedException($"Cannot resolve a command from '{nextOutcome.GetType().Name}'.");
    }

    private Task TriggerCommandReactions(Outcome[] outcomes)
    {
        var providedReactions = outcomes
            .OfType<ReactionOutcome>()
            .Select(outcome => outcome.Reaction);

        var specifiedReactions = outcomes
            .OfType<SpecifiedReactionOutcome>()
            .Select(outcome => GetReactionToPublish(outcome, outcomes));

        var publishTasks = providedReactions
            .Concat(specifiedReactions)
            .Select(reaction => _publisher.Publish(reaction))
            .ToList();

        return Task.WhenAll(publishTasks);
    }

    /// <summary>
    /// The reaction to publish for a specified outcome. A provided outcome already carries one; a
    /// specified outcome names a type, which the run resolves through the reaction factory path, so the
    /// factory sees the run's artefacts — those accumulated before this command plus the outcomes it
    /// just produced.
    /// </summary>
    private CliCommandReaction GetReactionToPublish(SpecifiedReactionOutcome specifiedOutcome, Outcome[] currentOutcomes)
        => _workflowReactionProvider.GetReaction(
            specifiedOutcome.SpecifiedReactionType,
            [..AllPriorOutcomes(), ..currentOutcomes]);

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
            ClIWorkflowRunStateStatus.Exceptional,
            ClIWorkflowRunStateStatus.InvalidMovePastAsk);

        if (runComplete)
        {
            State.ChangeTo(ClIWorkflowRunStateStatus.Finished);
            _serviceScope.Dispose();
        }
    }

    /// <summary>
    /// Turns down an ask that never got as far as resolving a command. A run parked at a reusable
    /// checkpoint keeps its place — it makes no state change and suggests what would work instead,
    /// as it does for an ask naming no command. Any other run fails into <c>InvalidAsk</c> and finishes.
    /// </summary>
    private Outcome[] RejectAsk()
    {
        if (State.WasChangedTo(ClIWorkflowRunStateStatus.ReachedReusableOutcome))
        {
            return SuggestNextCommands(AllPriorOutcomes());
        }

        State.ChangeTo(ClIWorkflowRunStateStatus.InvalidAsk);
        UpdateStateWhenFinished();

        return [new NothingOutcome()];
    }

    private Outcome[] SuggestNextCommands(List<Outcome> priorOutcomes)
    {
        var lastRanCommandType = priorOutcomes
            .OfType<RanCliCommandOutcome>()
            .LastOrDefault()?
            .Command
            .GetType();

        var possibleCommandsToMoveTo = lastRanCommandType?
            .GetCliNextCommandNames()
            .ToList() ?? [];

        if (possibleCommandsToMoveTo.Count == 0)
        {
            return [new NothingOutcome()];
        }

        var prefix = _instructionSettings.Prefix.ToString();

        return possibleCommandsToMoveTo
            .Select(Outcome (next) => new SuggestionOutcome(
                $"{prefix}{next.Name}",
                next.Description))
            .ToArray();
    }

    // TODO: Perhaps move to extension somewhere
    private List<Outcome> AllPriorOutcomes()
        => State
            .AllOutcomeStateChanges()
            .SelectMany(change => change.Outcomes)
            .ToList();
}
