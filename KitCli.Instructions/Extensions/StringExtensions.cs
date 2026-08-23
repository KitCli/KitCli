using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Extensions;

/// <summary>
/// Provides string helper extension methods used during instruction token extraction and argument building.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether the string contains any letter characters, ignoring whitespace.
    /// </summary>
    /// <param name="argumentValue">The string to inspect.</param>
    /// <returns><see langword="true"/> if the string contains at least one letter; otherwise, <see langword="false"/>.</returns>
    public static bool AnyLetters(this string argumentValue)
        => argumentValue
            .ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .Any(char.IsLetter);

    /// <summary>
    /// Extracts the substring of terminal input covered by the given token index.
    /// </summary>
    /// <param name="terminalInput">The full terminal input string.</param>
    /// <param name="tokenIndex">The token index identifying the substring's start and end positions.</param>
    /// <returns>The substring of <paramref name="terminalInput"/> spanning the token's range.</returns>
    public static string ExtractTokenContent(this string terminalInput, InstructionTokenIndex tokenIndex)
        => terminalInput[tokenIndex.StartIndex..tokenIndex.EndIndex];
}