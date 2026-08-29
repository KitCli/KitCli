using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A custom artefact factory the assembly-scanning registration should discover.</summary>
public class TestArtefactFactory : ArtefactFactory<SayOutcome>
{
    protected override AnonymousArtefact CreateArtefact(SayOutcome outcome)
        => new TestArtefact(outcome.Something);
}
