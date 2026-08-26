using KitCli.Commands.Abstractions.Factories;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A decision factory that applies only to the sub-instruction it is named for.</summary>
public class TestDecisionCliCommandFactory : BasicDecisionCliCommandFactory<TestVariantNextCliCommand>
{
    public const string AppliesToSubCommandName = "next";

    public override bool CanCreateWhen() => SubCommandIs(AppliesToSubCommandName);
}
