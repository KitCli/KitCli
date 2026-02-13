using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Artefacts;

public interface IArtefactFactory
{
    bool For(Outcome outcome);
    
    AnonymousArtefact Create(Outcome outcome);
}