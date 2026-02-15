using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;

namespace KitCli.Instructions.Builders;

internal class DirectoryInfoInstructionArgumentBuilder : IInstructionArgumentBuilder
{
    public bool For(string? argumentValue)
    {
        if (string.IsNullOrEmpty(argumentValue))
        {
            return false;
        }

        return IsFilePath(argumentValue);
    }

    public AnonymousInstructionArgument Create(string argumentName, string? argumentValue)
    {
        var directoryInfo = new DirectoryInfo(argumentValue ?? string.Empty);
        
        return new InstructionArgument<DirectoryInfo>(argumentName, directoryInfo);
    }
    
    private static bool IsFilePath(string argumentValue)
    {
        return Path.IsPathRooted(argumentValue) || argumentValue.StartsWith($".");
    }
}