using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io;

public interface ICliCommandOutcomeIoWriter
{
    bool CanWriteFor(CliCommandOutcome outcome);
    
    void Write(CliCommandOutcome outcome);
}