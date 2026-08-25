namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A reaction a handler can publish as a side effect.</summary>
public record TestCliCommandReaction(string Because) : CliCommandReaction;
