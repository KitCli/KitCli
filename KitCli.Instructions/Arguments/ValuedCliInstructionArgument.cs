using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Arguments;

public class ValuedCliInstructionArgument<TArgumentValue>(string name, TArgumentValue argumentValue)
    : CliInstructionArgument(name) where TArgumentValue : notnull
{
    public TArgumentValue ArgumentValue { get; } = argumentValue;
}