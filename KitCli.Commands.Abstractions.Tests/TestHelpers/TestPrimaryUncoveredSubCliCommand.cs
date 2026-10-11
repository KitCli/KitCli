namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A sub-command of <see cref="TestPrimaryCliCommand"/> with no factory at all — the uncovered sibling
/// the automatic fallback has to resolve on its own.
/// </summary>
public record TestPrimaryUncoveredSubCliCommand : CliCommand;
