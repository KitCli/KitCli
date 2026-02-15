using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class DecimalInstructionArgumentBuilder : InstructionArgumentBuilder, IInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => decimal.TryParse(argumentValue, out _);

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var validArgumentValue = GetValidValue(argumentName, argumentValue);
        var parsedArgumentValue = decimal.Parse(validArgumentValue);
        return new InstructionArgument<decimal>(argumentName, parsedArgumentValue);
    }
}