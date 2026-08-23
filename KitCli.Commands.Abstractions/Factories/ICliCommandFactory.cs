using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Instructions.Abstractions;

namespace KitCli.Commands.Abstractions.Factories;

/// <summary>
/// Builds a <see cref="CliCommand"/> instance for a resolved instruction, optionally using the current
/// instruction's arguments and the run's artefacts.
/// </summary>
public interface ICliCommandFactory
{
    /// <summary>
    /// Attaches the current instruction and artefact list to this factory before <see cref="CanCreateWhen"/>
    /// or <see cref="Create"/> is called.
    /// </summary>
    /// <param name="instruction">The instruction being resolved.</param>
    /// <param name="artefacts">The artefacts accumulated so far in the run.</param>
    /// <returns>This factory instance, for chaining.</returns>
    ICliCommandFactory Attach(Instruction instruction, List<AnonymousArtefact> artefacts);

    /// <summary>
    /// Determines whether this factory can create a command for the attached instruction and artefacts.
    /// </summary>
    /// <returns><see langword="true"/> if this factory can create a command right now; otherwise <see langword="false"/>.</returns>
    bool CanCreateWhen();

    /// <summary>
    /// Creates the command.
    /// </summary>
    /// <returns>The created command.</returns>
    CliCommand Create();
}