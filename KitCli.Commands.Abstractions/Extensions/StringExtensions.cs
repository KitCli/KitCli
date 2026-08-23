namespace KitCli.Commands.Abstractions.Extensions;

/// <summary>
/// String helpers for deriving command instruction names and shorthands from PascalCase type names.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Inserts <paramref name="separator"/> before every uppercase letter (except the first) and lowercases
    /// the result — e.g. turning <c>SpareMoney</c> into <c>spare-money</c> when <paramref name="separator"/> is <c>-</c>.
    /// </summary>
    /// <param name="str">The PascalCase string to convert.</param>
    /// <param name="separator">The separator to insert before each uppercase letter.</param>
    /// <returns>The lowercased, separator-delimited string.</returns>
    public static string ToLowerSplitString(this string str, char separator)
    {
        var characters = str.ToCharArray();

        var splitCharacters = SplitCharacters(characters, separator);
        
        var joinedCharacters = new string(splitCharacters.ToArray());

        var escapedJoinedCharacters = joinedCharacters[1..];

        return escapedJoinedCharacters.ToLower();
    }
    
    /// <summary>
    /// Keeps only the uppercase letters of the given string and lowercases them — e.g. turning
    /// <c>SpareMoney</c> into <c>sm</c>, the shorthand form of a command's instruction name.
    /// </summary>
    /// <param name="str">The string to extract uppercase letters from.</param>
    /// <returns>The lowercased uppercase-only characters of <paramref name="str"/>.</returns>
    public static string ToLowerTitleCharacters(this string str)
    {
        var characters = str.ToCharArray();
        
        var charactersIndexed = characters.ToList();
        
        var upperCaseCharacters = charactersIndexed.Where(char.IsUpper);
        
        var joinedUpperCaseCharacters = new string(upperCaseCharacters.ToArray());

        return joinedUpperCaseCharacters.ToLower();
    }
    
    private static IEnumerable<char> SplitCharacters(char[] characters, char separator)
    {
        foreach (var currentCharacter in characters)
        {
            if (char.IsUpper(currentCharacter))
            {
                yield return separator;
            }
                
            yield return currentCharacter;
        }
    }
}