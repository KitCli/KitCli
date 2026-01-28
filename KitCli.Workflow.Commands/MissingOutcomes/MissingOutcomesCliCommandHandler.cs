using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Commands.MissingOutcomes;

// TODO: Write uit tests.
public class MissingOutcomesCliCommandHandler : CliCommandHandler, ICliCommandHandler<MissingOutcomesCliCommand>
{
    private const string Message = "The following prerequisite outcomes were not returned from previous commands:";
    
    public Task<CliCommandOutcome[]> Handle(MissingOutcomesCliCommand command, CancellationToken cancellationToken)
    {
        var missingOutcomeList = string.Join(", ", command.MissingOutcomeNames);
        return AsyncOutcomeAs($"{Message} {missingOutcomeList}");
    }
}