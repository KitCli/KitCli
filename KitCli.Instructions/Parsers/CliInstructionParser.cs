using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Parsers;

public class CliInstructionParser : ICliInstructionParser
{
    private readonly CliInstructionTokenIndexer _cliInstructionTokenIndexer;
    private readonly CliInstructionTokenExtractor _cliInstructionTokenExtractor;
    private readonly IEnumerable<ICliInstructionArgumentBuilder> _instructionArgumentBuilders;

    public CliInstructionParser(
        CliInstructionTokenIndexer cliInstructionTokenIndexer,
        CliInstructionTokenExtractor cliInstructionTokenExtractor,
        IEnumerable<ICliInstructionArgumentBuilder> instructionArgumentBuilders)
    {
        _cliInstructionTokenIndexer = cliInstructionTokenIndexer;
        _cliInstructionTokenExtractor = cliInstructionTokenExtractor;
        _instructionArgumentBuilders = instructionArgumentBuilders;
    }

    public CliInstruction Parse(string terminalInput)
    {
        var indexes = _cliInstructionTokenIndexer.Index(terminalInput);
        var extraction = _cliInstructionTokenExtractor.Extract(indexes, terminalInput);

        var arguments = extraction
            .ArgumentTokens
            .Select(token => _instructionArgumentBuilders
                .First(builder => builder.For(token.Value))
                .Create(token.Key, token.Value))
            .ToList();
        
        return new CliInstruction(
            extraction.PrefixToken,
            extraction.NameToken,
            extraction.SubNameToken,
            arguments);
    }
}