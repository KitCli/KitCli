namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A sub-command of <see cref="TestPrimaryCliCommand"/> with a hand-written factory,
/// <see cref="TestPrimaryDedicatedSubCliCommandFactory"/>.
/// </summary>
public record TestPrimaryDedicatedSubCliCommand : CliCommand;
