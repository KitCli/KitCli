using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.RanCliCommand;

public class RanCliCommandArtefactFactory : ArtefactFactory<RanCliCommandOutcome>
{
    protected override AnonymousArtefact CreateArtefact(RanCliCommandOutcome cliCommandOutcome)
        => new RanCliCommandArtefact(cliCommandOutcome.Command);
}