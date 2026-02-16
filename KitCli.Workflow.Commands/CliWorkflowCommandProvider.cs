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
        var commandFactories = serviceProvider
            .GetKeyedServices<ICliCommandFactory>(instruction.Name)
            .ToList();
        
        if (commandFactories.Count == 0)
        {
            throw new NoCommandGeneratorException("Did not find generator for " + instruction.Name);
        }
        
        var artefacts = ConvertOutcomesToArtefacts(outcomes);
        
        var commandFactory = commandFactories
            .Select(commandFactory => commandFactory.Attach(instruction, artefacts))
            .FirstOrDefault(commandFactory => commandFactory.CanCreateWhen());
        
        if (commandFactory == null)
        {
            throw new NoCommandGeneratorException("Did not find command factory for " + instruction.Name);
        }

        return commandFactory.Create();
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