namespace KitCli.Instructions.Abstractions;

/// <summary>
/// Represents an argument parsed from an instruction before it has been resolved
/// to a concrete typed argument.
/// </summary>
/// <param name="Name">The name of the argument as it appeared in the instruction text.</param>
public record AnonymousInstructionArgument(string Name);