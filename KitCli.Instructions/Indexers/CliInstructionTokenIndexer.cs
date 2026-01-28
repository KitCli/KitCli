using KitCli.Instructions.Abstractions;
using Microsoft.Extensions.Options;

namespace KitCli.Instructions.Indexers;

public class CliInstructionTokenIndexer(IOptions<InstructionSettings> instructionOptions)
{
    private readonly InstructionSettings _instructionSettings = instructionOptions.Value;

    public CliInstructionTokenIndexCollection Index(string terminalInput)
    {
        if (string.IsNullOrEmpty(terminalInput))
        {
            return CreateEmptyCollection();
        }

        var prefixIndex = IndexPrefixToken(terminalInput);
        var nameIndex = IndexNameToken(terminalInput, prefixIndex.EndIndex);
        var argumentsIndex = IndexArgumentsToken(terminalInput);
        var subNameIndex = IndexSubNameToken(terminalInput, argumentsIndex);
        
        return new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = prefixIndex,
            [CliInstructionTokenType.Name] = nameIndex,
            [CliInstructionTokenType.SubName] = subNameIndex,
            [CliInstructionTokenType.Arguments] = argumentsIndex
        };
    }

    private static CliInstructionTokenIndexCollection CreateEmptyCollection()
    {
        return new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex { Found = false },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex { Found = false },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex { Found = false },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex { Found = false }
        };
    }

    private CliInstructionTokenIndex IndexPrefixToken(string terminalInput)
    {
        // Constraint: Command must be prefixed with some kind of mark, e.g. /
        var firstPunctuationMark = terminalInput
            .ToCharArray()
            .FirstOrDefault(character => character == _instructionSettings.Prefix);
        
        // e.g. <here>/spare-money help --argumentOne hello world --argumentTwo 1
        var firstPunctuationMarkIndex = terminalInput.IndexOf(firstPunctuationMark);
        var hasFirstPunctuationMark = firstPunctuationMarkIndex == 0;
        
        return new CliInstructionTokenIndex
        {
            Found = hasFirstPunctuationMark,
            StartIndex = firstPunctuationMarkIndex,
            EndIndex = firstPunctuationMarkIndex + 1
        };
    }

    private static CliInstructionTokenIndex IndexNameToken(string terminalInput, int afterPunctuationMarkIndex)
    {
        // e.g. /<here>spare-money help --argumentOne hello world --argumentTwo 1
        var firstSpaceIndex = terminalInput.IndexOf(CliInstructionConstants.DefaultSpaceCharacter);
        var hasFirstSpace = firstSpaceIndex != -1;
        
        // If command name is present, the first space should not be immediately after the /
        var hasCommandNameToken = firstSpaceIndex != afterPunctuationMarkIndex;
        
        // e.g. /spare-money<here> help --argumentOne hello world --argumentTwo 1
        var commandNameTokenEndIndex = hasFirstSpace ? firstSpaceIndex : terminalInput.Length;
        
        return new CliInstructionTokenIndex
        {
            Found = hasCommandNameToken,
            StartIndex = afterPunctuationMarkIndex,
            EndIndex = commandNameTokenEndIndex
        };
    }

    private CliInstructionTokenIndex IndexArgumentsToken(string terminalInput)
    {
        // e.g. /spare-money help <here>--argumentOne hello world --argumentTwo 1
        var firstArgumentIndex = terminalInput.IndexOf(
            _instructionSettings.ArgumentPrefix,
            StringComparison.Ordinal);
        
        var hasArgumentTokens = firstArgumentIndex != -1;
        
        return new CliInstructionTokenIndex
        {
            Found = hasArgumentTokens,
            StartIndex = firstArgumentIndex,
            EndIndex = terminalInput.Length
        };
    }

    private static CliInstructionTokenIndex IndexSubNameToken(
        string terminalInput, 
        CliInstructionTokenIndex argumentsIndex)
    {
        var firstSpaceIndex = terminalInput.IndexOf(CliInstructionConstants.DefaultSpaceCharacter);
        var hasFirstSpace = firstSpaceIndex != -1;
        
        if (!hasFirstSpace)
        {
            return new CliInstructionTokenIndex { Found = false };
        }

        var beforeFirstArgumentIndex = argumentsIndex.StartIndex - 1;
        var inputBetweenFirstSpaceAndFirstArgument = beforeFirstArgumentIndex != firstSpaceIndex;
        
        // The space between the first argument and command name has nothing in it - if there is a space at all
        var hasSubCommandNameToken = inputBetweenFirstSpaceAndFirstArgument;
        
        // e.g. /spare-money <here>help --argumentOne hello world --argumentTwo 1
        var subCommandNameStartIndex = firstSpaceIndex + 1;
        
        // e.g. /spare-money help<here> --argumentOne hello world --argumentTwo 1
        var subCommandNameEndIndex = argumentsIndex.Found ? argumentsIndex.StartIndex - 1 : terminalInput.Length;
        
        return new CliInstructionTokenIndex
        {
            Found = hasSubCommandNameToken,
            StartIndex = subCommandNameStartIndex,
            EndIndex = subCommandNameEndIndex
        };
    }
}