namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A command created only for the sub-instruction its factory decides it applies to, as against
/// <see cref="TestNextCliCommand"/>, whose factory always applies.
/// </summary>
public record TestVariantNextCliCommand : CliCommand;
