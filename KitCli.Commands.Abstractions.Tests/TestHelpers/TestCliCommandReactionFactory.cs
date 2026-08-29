using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>Builds <see cref="TestFactoryBuiltCliCommandReaction"/>, exercising the dedicated-factory registration path.</summary>
public class TestCliCommandReactionFactory : CliCommandReactionFactory<TestFactoryBuiltCliCommandReaction>
{
    public override bool CanCreateWhen() => true;

    public override CliCommandReaction Create() => new TestFactoryBuiltCliCommandReaction("built by factory");
}
