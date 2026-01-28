using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;

namespace KitCli.Commands.Abstractions.Io.Outcomes;

public interface ICliCommandOutcomeIo : ICliIo
{
    void Say(CliCommandOutcome[] outcomes);
}