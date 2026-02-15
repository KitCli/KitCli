using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class BoolInstructionArgumentBuilder : IInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => true;

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        if (bool.TryParse(argumentValue, out var argumentBool))
        {
            return new InstructionArgument<bool>(argumentName, argumentBool);
        }
        
        return new InstructionArgument<bool>(argumentName, true);
    }
}