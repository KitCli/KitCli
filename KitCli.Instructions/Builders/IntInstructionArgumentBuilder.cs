using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class IntInstructionArgumentBuilder : InstructionArgumentBuilder, IInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => argumentValue != null && int.TryParse(argumentValue, out _);

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var validArgumentValue = GetValidValue(argumentName, argumentValue);
        var parsedArgumentValue = int.Parse(validArgumentValue);
        return new InstructionArgument<int>(argumentName, parsedArgumentValue);
    }
}