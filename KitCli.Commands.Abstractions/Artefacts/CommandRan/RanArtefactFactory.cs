using KitCli.Commands.Abstractions.Outcomes.Reusable;

namespace KitCli.Commands.Abstractions.Artefacts.CommandRan;

public class RanArtefactFactory : ArtefactFactory<RanOutcome>
{
    protected override AnonymousArtefact CreateArtefact(RanOutcome outcome)
        => new RanCliCommandAnonymousArtefact(outcome.Command);
}