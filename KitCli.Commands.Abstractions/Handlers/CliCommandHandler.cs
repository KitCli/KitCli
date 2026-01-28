using KitCli.Abstractions.Aggregators.Filters;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;

namespace KitCli.Commands.Abstractions.Handlers;

public abstract class CliCommandHandler
{
    protected static CliCommandOutcome[] OutcomeAs()
        => [new CliCommandNothingOutcome()];
    
    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs()
        => Task.FromResult(OutcomeAs());
    
    protected static CliCommandOutcome[] OutcomeAs(CliTable cliTable)
        => [new CliCommandTableOutcome(cliTable)];

    protected static CliCommandOutcome[] OutcomeAs(string message)
        => [new CliCommandOutputOutcome(message)];
    
    protected static CliCommandOutcome[] OutcomeAs(params string[] messages)
        => messages
            .Select(message => new CliCommandOutputOutcome(message))
            .ToArray<CliCommandOutcome>();

    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs(string message)
        => Task.FromResult(OutcomeAs(message));
    
    protected static CliCommandOutcome[] OutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => [new FilterCliCommandOutcome(cliListAggregatorFilter)];
    
    protected static Task<CliCommandOutcome[]> AsyncOutcomeAs(CliListAggregatorFilter cliListAggregatorFilter)
        => Task.FromResult(OutcomeAs(cliListAggregatorFilter));
}