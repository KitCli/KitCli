using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class DateOnlyInstructionArgumentBuilder : InstructionArgumentBuilder, IInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => DateTime.TryParse(argumentValue, out _);

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var validArgumentValue = GetValidValue(argumentName, argumentValue);
        var argumentDate = DateTime.Parse(validArgumentValue);
        var argumentDateOnly = DateOnly.FromDateTime(argumentDate);

        return new InstructionArgument<DateOnly>(argumentName, argumentDateOnly);
    }
}