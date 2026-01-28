using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Extensions;

namespace KitCli.Instructions.Builders;

internal class StringCliInstructionArgumentBuilder : CliInstructionArgumentBuilder, ICliInstructionArgumentBuilder
{
    public bool For(string? argumentValue)
    {
        if (argumentValue == null) return false;

        return argumentValue.AnyLetters() && !bool.TryParse(argumentValue, out _);
    }

    public CliInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var validArgumentValue = GetValidValue(argumentName, argumentValue);
        return new ValuedCliInstructionArgument<string>(argumentName, validArgumentValue);
    }
}