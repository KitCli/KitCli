using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Arguments;

public class InstructionArgument<TArgumentValue>(string name, TArgumentValue value)
    : AnonymousInstructionArgument(name) where TArgumentValue : notnull
{
    public TArgumentValue Value { get; } = value;
}