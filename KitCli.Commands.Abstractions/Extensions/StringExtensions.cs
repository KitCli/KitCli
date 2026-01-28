namespace KitCli.Commands.Abstractions.Extensions;

public static class StringExtensions
{
    public static string ToLowerSplitString(this string str, char separator)
    {
        var characters = str.ToCharArray();

        var splitCharacters = SplitCharacters(characters, separator);
        
        var joinedCharacters = new string(splitCharacters.ToArray());

        var escapedJoinedCharacters = joinedCharacters[1..];

        return escapedJoinedCharacters.ToLower();
    }
    
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