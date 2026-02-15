using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Parsers;

public class InstructionParser : IInstructionParser
{
    private readonly InstructionTokenIndexer _instructionTokenIndexer;
    private readonly InstructionTokenExtractor _instructionTokenExtractor;
    private readonly IEnumerable<IInstructionArgumentBuilder> _instructionArgumentBuilders;

    public InstructionParser(
        InstructionTokenIndexer instructionTokenIndexer,
        InstructionTokenExtractor instructionTokenExtractor,
        IEnumerable<IInstructionArgumentBuilder> instructionArgumentBuilders)
    {
        _instructionTokenIndexer = instructionTokenIndexer;
        _instructionTokenExtractor = instructionTokenExtractor;
        _instructionArgumentBuilders = instructionArgumentBuilders;
    }

    public Instruction Parse(string terminalInput)
    {
        var indexes = _instructionTokenIndexer.Index(terminalInput);
        var extraction = _instructionTokenExtractor.Extract(indexes, terminalInput);

        var arguments = extraction
            .ArgumentTokens
            .Select(token => _instructionArgumentBuilders
                .First(builder => builder.For(token.Value))
                .Create(token.Key, token.Value))
            .ToList();
        
        return new Instruction(
            extraction.PrefixToken,
            extraction.NameToken,
            extraction.SubNameToken,
            arguments);
    }
}