using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class BoolCliInstructionArgumentBuilder : ICliInstructionArgumentBuilder
{
    public bool For(string? argumentValue) => true;

    public CliInstructionArgument Create(string argumentName, string? argumentValue)
    {
        if (bool.TryParse(argumentValue, out var argumentBool))
        {
            return new ValuedCliInstructionArgument<bool>(argumentName, argumentBool);
        }
        
        return new ValuedCliInstructionArgument<bool>(argumentName, true);
    }
}