using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

public interface IOutcomeIoWriter
{
    bool CanWriteFor(Outcome outcome);
    
    void Write(Outcome outcome);
}