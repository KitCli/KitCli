namespace KitCli.Commands.Abstractions.Extensions;

public static class CommandStringExtensions
{
    private const string CommandSuffix = nameof(CliCommand);
    
    public static string ReplaceCommandSuffix(this string something)
        => something.Replace(CommandSuffix, string.Empty);
}