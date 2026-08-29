using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Workflow.Commands;

/// <summary>
/// Resolves the <see cref="CliCommandReaction"/> that should be published for a specified reaction type.
/// </summary>
public interface ICliWorkflowReactionProvider
{
    /// <summary>
    /// Resolves the reaction to publish for the given reaction type, taking prior outcomes into
    /// account when selecting among registered reaction factories.
    /// </summary>
    /// <param name="reactionType">The reaction type to resolve a reaction for.</param>
    /// <param name="outcomes">Outcomes produced earlier in the workflow, used to inform reaction creation.</param>
    /// <returns>The resolved <see cref="CliCommandReaction"/>.</returns>
    /// <exception cref="NoReactionFactoryException">
    /// Thrown when no reaction factory is registered for the reaction type, or none of the
    /// registered factories can create a reaction for it.
    /// </exception>
    CliCommandReaction GetReaction(Type reactionType, List<Outcome> outcomes);
}
