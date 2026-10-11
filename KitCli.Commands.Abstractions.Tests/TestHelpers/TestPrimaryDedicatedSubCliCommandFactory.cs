using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A hand-written factory for <see cref="TestPrimaryDedicatedSubCliCommand"/>, declared on
/// <see cref="TestPrimaryCliCommand"/>'s key per the shared sub-command pattern.
/// </summary>
public class TestPrimaryDedicatedSubCliCommandFactory : CliCommandFactory<TestPrimaryCliCommand>
{
    public const string AppliesToSubCommandName = "dedicated-sub";

    public override bool CanCreateWhen() => SubCommandIs(AppliesToSubCommandName);

    public override CliCommand Create() => new TestPrimaryDedicatedSubCliCommand();
}
