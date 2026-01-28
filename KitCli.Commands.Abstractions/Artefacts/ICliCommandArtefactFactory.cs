using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

public interface ICliCommandArtefactFactory
{
    bool For(CliCommandOutcome outcome);
    
    CliCommandArtefact Create(CliCommandOutcome outcome);
}