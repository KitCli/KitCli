using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow.Commands;

/// <summary>
/// Resolves a <see cref="CliCommandReaction"/> for a specified reaction type by selecting the first
/// registered <see cref="ICliCommandReactionFactory"/> that can create it, converting prior outcomes
/// into artefacts the factory can use to decide.
/// </summary>
/// <param name="serviceProvider">Provider used to resolve keyed reaction factories and artefact factories.</param>
public class CliWorkflowReactionProvider(IServiceProvider serviceProvider) : ICliWorkflowReactionProvider
{
    /// <inheritdoc/>
    public CliCommandReaction GetReaction(Type reactionType, List<Outcome> outcomes)
    {
        var reactionFactories = serviceProvider
            .GetKeyedServices<ICliCommandReactionFactory>(reactionType)
            .ToList();

        if (reactionFactories.Count == 0)
        {
            throw new NoReactionFactoryException("Did not find factory for " + reactionType.Name);
        }

        var artefacts = ConvertOutcomesToArtefacts(outcomes);

        var reactionFactory = reactionFactories
            .Select(reactionFactory => reactionFactory.Attach(artefacts))
            .FirstOrDefault(reactionFactory => reactionFactory.CanCreateWhen());

        if (reactionFactory == null)
        {
            throw new NoReactionFactoryException("Did not find reaction factory for " + reactionType.Name);
        }

        return reactionFactory.Create();
    }

    private List<AnonymousArtefact> ConvertOutcomesToArtefacts(List<Outcome> priorOutcomes)
    {
        var artefactFactories = serviceProvider.GetServices<IArtefactFactory>();

        var convertableOutcomes = priorOutcomes
            .Where(priorOutcome => artefactFactories
                .Any(artefactFactory => artefactFactory.For(priorOutcome)));

        return convertableOutcomes
            .Select(priorOutcome => artefactFactories
                .First(artefactFactory => artefactFactory.For(priorOutcome))
                .Create(priorOutcome))
            .ToList();
    }
}
