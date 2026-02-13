using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

public interface ICliCommandArtefactFactory
{
    bool For(Outcome outcome);
    
    CliCommandArtefact Create(Outcome outcome);
}