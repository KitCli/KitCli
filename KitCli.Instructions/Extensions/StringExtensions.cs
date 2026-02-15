using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Extensions;

public static class StringExtensions
{
    public static bool AnyLetters(this string argumentValue)
        => argumentValue
            .ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .Any(char.IsLetter);

    public static string ExtractTokenContent(this string terminalInput, InstructionTokenIndex tokenIndex)
        => terminalInput[tokenIndex.StartIndex..tokenIndex.EndIndex];
}