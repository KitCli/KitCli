namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A command with no factory of its own, shared as a registration key by
/// <see cref="TestPrimaryDedicatedSubCliCommand"/> and <see cref="TestPrimaryUncoveredSubCliCommand"/>.
/// </summary>
public record TestPrimaryCliCommand : CliCommand;
