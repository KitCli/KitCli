namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A reaction built by <see cref="TestCliCommandReactionFactory"/> rather than by constructor.</summary>
public record TestFactoryBuiltCliCommandReaction(string Because) : CliCommandReaction;
