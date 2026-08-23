using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Arguments;

/// <summary>
/// A strongly-typed instruction argument parsed from terminal input.
/// </summary>
/// <typeparam name="TArgumentValue">The type of the argument's parsed value.</typeparam>
/// <param name="Name">The argument's name, as it appeared on the command line.</param>
/// <param name="Value">The argument's parsed value.</param>
public record InstructionArgument<TArgumentValue>(string Name, TArgumentValue Value)
    : AnonymousInstructionArgument(Name) where TArgumentValue : notnull;