using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Commands.MissingOutcomes;

// TODO: Write uit tests.

// TODO: Revisit strategy for reporting missing outcomes.
internal class MissingOutcomesCliCommandHandler : CliCommandHandler<MissingOutcomesCliCommand>
{
    private const string Message = "The following prerequisite outcomes were not returned from previous commands:";

    public override Task<Outcome[]> HandleCommand(MissingOutcomesCliCommand command, CancellationToken cancellationToken)
    {
        var missingOutcomeList = string.Join(", ", command.MissingOutcomeNames);
        return AsyncOutcomeAs($"{Message} {missingOutcomeList}");
    }
}