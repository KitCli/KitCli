using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.Options;

namespace KitCli.Instructions.Indexers;

public class InstructionTokenIndexer(IOptions<InstructionSettings> instructionOptions)
{
    private readonly InstructionSettings _instructionSettings = instructionOptions.Value;

    public InstructionTokenIndexCollection Index(string terminalInput)
    {
        if (string.IsNullOrEmpty(terminalInput))
        {
            return CreateEmptyCollection();
        }

        var prefixIndex = IndexPrefixToken(terminalInput);
        var nameIndex = IndexNameToken(terminalInput, prefixIndex.EndIndex);
        var argumentsIndex = IndexArgumentsToken(terminalInput);
        var subNameIndex = IndexSubNameToken(terminalInput, argumentsIndex);
        
        return new InstructionTokenIndexCollection
        {
            [InstructionTokenType.Prefix] = prefixIndex,
            [InstructionTokenType.Name] = nameIndex,
            [InstructionTokenType.SubName] = subNameIndex,
            [InstructionTokenType.Arguments] = argumentsIndex
        };
    }

    private static InstructionTokenIndexCollection CreateEmptyCollection()
    {
        return new InstructionTokenIndexCollection
        {
            [InstructionTokenType.Prefix] = new InstructionTokenIndex { Found = false },
            [InstructionTokenType.Name] = new InstructionTokenIndex { Found = false },
            [InstructionTokenType.SubName] = new InstructionTokenIndex { Found = false },
            [InstructionTokenType.Arguments] = new InstructionTokenIndex { Found = false }
        };
    }

    private InstructionTokenIndex IndexPrefixToken(string terminalInput)
    {
        // Constraint: Command must be prefixed with some kind of mark, e.g. /
        var firstPunctuationMark = terminalInput
            .ToCharArray()
            .FirstOrDefault(character => character == _instructionSettings.Prefix);
        
        // e.g. <here>/spare-money help --argumentOne hello world --argumentTwo 1
        var firstPunctuationMarkIndex = terminalInput.IndexOf(firstPunctuationMark);
        var hasFirstPunctuationMark = firstPunctuationMarkIndex == 0;
        
        return new InstructionTokenIndex
        {
            Found = hasFirstPunctuationMark,
            StartIndex = firstPunctuationMarkIndex,
            EndIndex = firstPunctuationMarkIndex + 1
        };
    }

    private static InstructionTokenIndex IndexNameToken(string terminalInput, int afterPunctuationMarkIndex)
    {
        // e.g. /<here>spare-money help --argumentOne hello world --argumentTwo 1
        var firstSpaceIndex = terminalInput.IndexOf(InstructionConstants.DefaultSpaceCharacter);
        var hasFirstSpace = firstSpaceIndex != -1;
        
        // If command name is present, the first space should not be immediately after the /
        var hasCommandNameToken = firstSpaceIndex != afterPunctuationMarkIndex;
        
        // e.g. /spare-money<here> help --argumentOne hello world --argumentTwo 1
        var commandNameTokenEndIndex = hasFirstSpace ? firstSpaceIndex : terminalInput.Length;
        
        return new InstructionTokenIndex
        {
            Found = hasCommandNameToken,
            StartIndex = afterPunctuationMarkIndex,
            EndIndex = commandNameTokenEndIndex
        };
    }

    private InstructionTokenIndex IndexArgumentsToken(string terminalInput)
    {
        // e.g. /spare-money help <here>--argumentOne hello world --argumentTwo 1
        var firstArgumentIndex = terminalInput.IndexOf(
            _instructionSettings.ArgumentPrefix,
            StringComparison.Ordinal);
        
        var hasArgumentTokens = firstArgumentIndex != -1;
        
        return new InstructionTokenIndex
        {
            Found = hasArgumentTokens,
            StartIndex = firstArgumentIndex,
            EndIndex = terminalInput.Length
        };
    }

    private static InstructionTokenIndex IndexSubNameToken(
        string terminalInput, 
        InstructionTokenIndex argumentsIndex)
    {
        var firstSpaceIndex = terminalInput.IndexOf(InstructionConstants.DefaultSpaceCharacter);
        var hasFirstSpace = firstSpaceIndex != -1;
        
        if (!hasFirstSpace)
        {
            return new InstructionTokenIndex { Found = false };
        }

        var beforeFirstArgumentIndex = argumentsIndex.StartIndex - 1;
        var inputBetweenFirstSpaceAndFirstArgument = beforeFirstArgumentIndex != firstSpaceIndex;
        
        // The space between the first argument and command name has nothing in it - if there is a space at all
        var hasSubCommandNameToken = inputBetweenFirstSpaceAndFirstArgument;
        
        // e.g. /spare-money <here>help --argumentOne hello world --argumentTwo 1
        var subCommandNameStartIndex = firstSpaceIndex + 1;
        
        // e.g. /spare-money help<here> --argumentOne hello world --argumentTwo 1
        var subCommandNameEndIndex = argumentsIndex.Found ? argumentsIndex.StartIndex - 1 : terminalInput.Length;
        
        return new InstructionTokenIndex
        {
            Found = hasSubCommandNameToken,
            StartIndex = subCommandNameStartIndex,
            EndIndex = subCommandNameEndIndex
        };
    }
}