using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Arguments;

public record InstructionArgument<TArgumentValue>(string Name, TArgumentValue Value)
    : AnonymousInstructionArgument(Name) where TArgumentValue : notnull;