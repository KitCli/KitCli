using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Workflow.Abstractions;

namespace KitCli.Workflow.Commands.Exit;

// TODO: Write unit tests.
/// <summary>
/// Handles <see cref="ExitCliCommand"/> by stopping the CLI workflow and returning a final outcome.
/// </summary>
/// <param name="cliWorkflow">The workflow to stop when the command is handled.</param>
public class ExitCliCommandHandler(ICliWorkflow cliWorkflow) : CliCommandHandler<ExitCliCommand>
{
    /// <inheritdoc/>
    public override Task<Outcome[]> HandleCommand(ExitCliCommand command, CancellationToken cancellationToken)
    {
        cliWorkflow.Stop();
        
        var outcome = new FinalSayOutcome("Exiting CLI workflow.");
        return Task.FromResult<Outcome[]>([outcome]);
    }
}