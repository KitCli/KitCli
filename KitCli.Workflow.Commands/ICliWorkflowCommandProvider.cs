using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;

namespace KitCli.Workflow.Commands;

/// <summary>
/// Resolves the <see cref="CliCommand"/> that should be executed for a given instruction.
/// </summary>
public interface ICliWorkflowCommandProvider
{
    /// <summary>
    /// Resolves the command to execute for the given instruction, taking prior outcomes into
    /// account when selecting among registered command factories.
    /// </summary>
    /// <param name="instruction">The instruction to resolve a command for.</param>
    /// <param name="outcomes">Outcomes produced earlier in the workflow, used to inform command selection.</param>
    /// <returns>The resolved <see cref="CliCommand"/>.</returns>
    /// <exception cref="NoCommandGeneratorException">
    /// Thrown when no command factory is registered for the instruction, or none of the
    /// registered factories can create a command for it.
    /// </exception>
    CliCommand GetCommand(Instruction instruction, List<Outcome> outcomes);
}