using KitCli.Abstractions.Aggregators.Filters;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Skippable;
using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

public abstract class CliCommandHandler<TCliCommand> : IRequestHandler<TCliCommand, Outcome[]> where TCliCommand : CliCommand
{
    public Task<Outcome[]> Handle(TCliCommand command, CancellationToken cancellationToken)
        => HandleCommand(command, cancellationToken);

    public virtual Task<Outcome[]> HandleCommand(TCliCommand command, CancellationToken cancellationToken) 
        => AsyncOutcomeAs($"No functionality for {command.GetSpecificCommandName()} base command");
    
    // TODO: Might be a better way to do this.
    protected static Outcome[] OutcomeAs()
        => [new NothingOutcome()];
    
    protected static Task<Outcome[]> AsyncOutcomeAs()
        => Task.FromResult(OutcomeAs());
    
    protected static Outcome[] OutcomeAs(Table table)
        => [new TableOutcome(table)];

    protected static Outcome[] OutcomeAs(string message)
        => [new FinalMessageOutcome(message)];
    
    protected static Outcome[] OutcomeAs(params string[] messages)
        => messages
            .Select(message => new FinalMessageOutcome(message))
            .ToArray<Outcome>();

    protected static Task<Outcome[]> AsyncOutcomeAs(string message)
        => Task.FromResult(OutcomeAs(message));
    
    protected static Outcome[] OutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => [new FilterOutcome(cliListAggregatorFilter)];
    
    protected static Task<Outcome[]> AsyncOutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => Task.FromResult(OutcomeAs(cliListAggregatorFilter));
}