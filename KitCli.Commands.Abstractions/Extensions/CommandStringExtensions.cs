namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// String helpers for deriving command names from <see cref="CliCommand"/> type names.
/// </summary>
public static class CommandStringExtensions
{
    private const string CommandSuffix = nameof(CliCommand);

    /// <summary>
    /// Removes every occurrence of <c>CliCommand</c> from the given string. Despite the name, this is
    /// not a trailing-suffix strip: the word is removed wherever it appears, and a name ending in a
    /// bare <c>Command</c> is left untouched.
    /// </summary>
    /// <param name="something">The string to remove the suffix from.</param>
    /// <returns><paramref name="something"/> with all occurrences of <c>CliCommand</c> removed.</returns>
    public static string ReplaceCommandSuffix(this string something)
        => something.Replace(CommandSuffix, string.Empty);
}