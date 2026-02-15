using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Extensions;
using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Extraction;

public class InstructionTokenExtractor
{
    public InstructionTokenExtraction Extract(
        InstructionTokenIndexCollection indexes, 
        string terminalInput)
    {
        var prefixToken = ExtractOptionalToken(indexes, terminalInput, InstructionTokenType.Prefix);
        var nameToken = ExtractOptionalToken(indexes, terminalInput, InstructionTokenType.Name);
        var subNameToken = ExtractOptionalToken(indexes, terminalInput, InstructionTokenType.SubName);
        var argumentTokens = ExtractArgumentTokens(indexes, terminalInput);
        
        return new InstructionTokenExtraction(prefixToken, nameToken, subNameToken, argumentTokens);
    }

    private string? ExtractOptionalToken(
        InstructionTokenIndexCollection indexes,
        string terminalInput,
        InstructionTokenType tokenType)
    {
        var tokenIndex = indexes[tokenType];
        
        if (!tokenIndex.Found)
        {
            return null;
        }

        return terminalInput.ExtractTokenContent(tokenIndex);
    }
    
    private static Dictionary<string, string?> ExtractArgumentTokens(
        InstructionTokenIndexCollection indexes, 
        string terminalInput)
    {
        var argumentIndex = indexes[InstructionTokenType.Arguments];
        
        if (!argumentIndex.Found)
        {
            return new Dictionary<string, string?>();
        }
        
        var argumentInput = terminalInput.ExtractTokenContent(argumentIndex);

        return argumentInput
            .Split(InstructionConstants.DefaultArgumentPrefix)
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Select(ParseArgumentInput)
            .ToDictionary(token => token.Key, token => token.Value);
    }
    
    private static KeyValuePair<string, string?> ParseArgumentInput(string terminalArgumentInput)
    {
        // e.g. --payee-name Subway Something Something
        var firstIndexOfSpace = terminalArgumentInput.IndexOf(InstructionConstants.DefaultSpaceCharacter);
        
        var argumentNameEndIndex = firstIndexOfSpace == -1
            ? terminalArgumentInput.Length
            : firstIndexOfSpace;

        var argumentName = terminalArgumentInput.Substring(0, argumentNameEndIndex);

        var argumentValue = firstIndexOfSpace == -1
            ? null
            : terminalArgumentInput[argumentNameEndIndex..].Trim();
        
        return new KeyValuePair<string, string?>(argumentName, argumentValue);
    }
}