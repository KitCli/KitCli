using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Commands.Exit;

// TODO: Write unit tests.
public class ExitCliCommandHandler(ICliWorkflow cliWorkflow) : CliCommandHandler, ICliCommandHandler<ExitCliCommand>
{
    public Task<CliCommandOutcome[]> Handle(ExitCliCommand command, CancellationToken cancellationToken)
    {
        cliWorkflow.Stop();
        
        var outcome = new CliCommandOutputOutcome("Exiting CLI workflow.");
        return Task.FromResult<CliCommandOutcome[]>([outcome]);
    }
}