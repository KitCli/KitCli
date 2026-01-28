using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Builders;

public interface ICliInstructionArgumentBuilder
{
    bool For(string? argumentValue);

    CliInstructionArgument Create(string argumentName, string? argumentValue);
}
