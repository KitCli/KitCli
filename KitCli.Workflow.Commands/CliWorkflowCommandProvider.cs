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
    public CliCommand GetCommand(CliInstruction instruction, List<Outcome> outcomes)
    {
        var generators = serviceProvider
            .GetKeyedServices<IUnidentifiedCliCommandFactory>(instruction.Name)
            .ToList();
        
        if (generators.Count == 0)
        {
            throw new NoCommandGeneratorException("Did not find generator for " + instruction.Name);
        }
        
        var artefacts = ConvertOutcomesToArtefacts(outcomes);

        var generator = generators.FirstOrDefault(g => g.CanCreateWhen(instruction, artefacts));
        if (generator == null)
        {
            throw new NoCommandGeneratorException("Did not find generator for " + instruction.Name);
        }

        return generator.Create(instruction, artefacts);
    }

    private List<CliCommandArtefact> ConvertOutcomesToArtefacts(List<Outcome> priorOutcomes)
    {
        var artefactFactories = serviceProvider.GetServices<ICliCommandArtefactFactory>();
        
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