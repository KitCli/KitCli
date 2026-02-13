using KitCli.Abstractions.Aggregators.Filters;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using MediatR;

namespace KitCli.Commands.Abstractions.Handlers;

public abstract class CliCommandHandler<TCliCommand> : IRequestHandler<TCliCommand, CliCommandOutcome[]> where TCliCommand : CliCommand
{
    public Task<CliCommandOutcome[]> Handle(TCliCommand command, CancellationToken cancellationToken)
        => HandleCommand(command, cancellationToken);

    public virtual Task<CliCommandOutcome[]> HandleCommand(TCliCommand command, CancellationToken cancellationToken) 
        => AsyncOutcomeAs($"No functionality for {command.GetSpecificCommandName()} base command");
    
    // TODO: Might be a better way to do this.
    protected static CliCommandOutcome[] OutcomeAs()
        => [new NothingCliCommandOutcome()];
    
    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs()
        => Task.FromResult(OutcomeAs());
    
    protected static CliCommandOutcome[] OutcomeAs(Table table)
        => [new TableCliCommandOutcome(table)];

    protected static CliCommandOutcome[] OutcomeAs(string message)
        => [new OutputCliCommandOutcome(message)];
    
    protected static CliCommandOutcome[] OutcomeAs(params string[] messages)
        => messages
            .Select(message => new OutputCliCommandOutcome(message))
            .ToArray<CliCommandOutcome>();

    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs(string message)
        => Task.FromResult(OutcomeAs(message));
    
    protected static CliCommandOutcome[] OutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => [new FilterCliCommandOutcome(cliListAggregatorFilter)];
    
    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => Task.FromResult(OutcomeAs(cliListAggregatorFilter));
}