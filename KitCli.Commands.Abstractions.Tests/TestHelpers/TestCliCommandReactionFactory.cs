using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// Builds <see cref="TestFactoryBuiltCliCommandReaction"/>, exercising the dedicated-factory registration
/// path, and surfaces <see cref="CliCommandReactionFactory{TReaction}"/>'s protected artefact resolution
/// to a test, under the same names the API declares them with.
/// </summary>
public class TestCliCommandReactionFactory : CliCommandReactionFactory<TestFactoryBuiltCliCommandReaction>
{
    public override bool CanCreateWhen() => true;

    public override CliCommandReaction Create() => new TestFactoryBuiltCliCommandReaction("built by factory");

    public new List<AnonymousArtefact> Artefacts => base.Artefacts;

    public new bool AnyArtefact<TArtefactType>(string? artefactName = null) where TArtefactType : notnull
        => base.AnyArtefact<TArtefactType>(artefactName);

    public new Artefact<TArtefactType>? GetArtefact<TArtefactType>(string? artefactName = null)
        where TArtefactType : notnull
        => base.GetArtefact<TArtefactType>(artefactName);

    public new Artefact<TArtefactType> GetRequiredArtefact<TArtefactType>(string? artefactName = null)
        where TArtefactType : notnull
        => base.GetRequiredArtefact<TArtefactType>(artefactName);

    public new IEnumerable<Artefact<TArtefactType>> GetArtefacts<TArtefactType>() where TArtefactType : notnull
        => base.GetArtefacts<TArtefactType>();
}
