using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

public interface IOutcomeIoWriter
{
    // TODO: Follow similar pattern to ArtefactFactory. 
    bool CanWriteFor(Outcome outcome);
    
    void Write(Outcome outcome);
}