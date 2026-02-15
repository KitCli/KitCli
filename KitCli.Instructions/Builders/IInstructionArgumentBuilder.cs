using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Builders;

public interface IInstructionArgumentBuilder
{
    bool For(string? argumentValue);

    AnonymousInstructionArgument Create(string argumentName, string? argumentValue);
}
