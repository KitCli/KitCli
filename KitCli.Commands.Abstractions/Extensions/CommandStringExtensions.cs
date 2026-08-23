namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// String helpers for deriving command names from <see cref="CliCommand"/> type names.
/// </summary>
public static class CommandStringExtensions
{
    private const string CommandSuffix = nameof(CliCommand);

    /// <summary>
    /// Removes every occurrence of the <c>Command</c> suffix from the given string.
    /// </summary>
    /// <param name="something">The string to remove the suffix from.</param>
    /// <returns><paramref name="something"/> with all occurrences of <c>Command</c> removed.</returns>
    public static string ReplaceCommandSuffix(this string something)
        => something.Replace(CommandSuffix, string.Empty);
}