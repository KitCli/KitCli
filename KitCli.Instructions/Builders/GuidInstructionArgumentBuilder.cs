using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class GuidInstructionArgumentBuilder : InstructionArgumentBuilder, IInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => Guid.TryParse(argumentValue, out _);

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var validArgumentValue = GetValidValue(argumentName, argumentValue);
        var parsedArgumentValue = Guid.Parse(validArgumentValue);
        return new InstructionArgument<Guid>(argumentName, parsedArgumentValue);
    }
}