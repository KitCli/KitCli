using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;

namespace KitCli.Workflow.Commands;

public interface ICliWorkflowCommandProvider
{
    CliCommand GetCommand(CliInstruction instruction, List<CliCommandOutcome> outcomes);
}