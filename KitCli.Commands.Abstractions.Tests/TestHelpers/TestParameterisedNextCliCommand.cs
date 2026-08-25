namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>
/// A command a handler can chain to that has no parameterless constructor — the shape
/// <c>AddCommandFactories</c> refuses to auto-register, and the one a dedicated factory exists for.
/// </summary>
public record TestParameterisedNextCliCommand(string Text) : CliCommand;
