using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace KitCli.Workflow.Commands;

public class CliWorkflowCommandProvider(IServiceProvider serviceProvider) : ICliWorkflowCommandProvider
{
    // TODO: Test cases for the exceptions.
    public CliCommand GetCommand(Instruction instruction, List<Outcome> outcomes)
    {
        var generators = serviceProvider
            .GetKeyedServices<ICliCommandFactory>(instruction.Name)
            .ToList();
        
        if (generators.Count == 0)
        {
            throw new NoCommandGeneratorException("Did not find generator for " + instruction.Name);
        }
        
        var artefacts = ConvertOutcomesToArtefacts(outcomes);
        
        var generator = generators
            .Select(g => g.Attach(instruction, artefacts))
            .FirstOrDefault(g => g.CanCreateWhen());
        
        if (generator == null)
        {
            throw new NoCommandGeneratorException("Did not find generator for " + instruction.Name);
        }

        return generator.Create();
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