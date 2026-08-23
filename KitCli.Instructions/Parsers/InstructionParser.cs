using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;

namespace KitCli.Instructions.Parsers;

/// <summary>
/// Default <see cref="IInstructionParser"/> implementation that indexes, extracts, and builds an
/// <see cref="Instruction"/> from raw terminal input.
/// </summary>
public class InstructionParser : IInstructionParser
{
    private readonly InstructionTokenIndexer _instructionTokenIndexer;
    private readonly InstructionTokenExtractor _instructionTokenExtractor;
    private readonly IEnumerable<IInstructionArgumentBuilder> _instructionArgumentBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionParser"/> class.
    /// </summary>
    /// <param name="instructionTokenIndexer">Locates token positions within terminal input.</param>
    /// <param name="instructionTokenExtractor">Extracts raw token strings using their positions.</param>
    /// <param name="instructionArgumentBuilders">The candidate builders used to parse each argument's value.</param>
    public InstructionParser(
        InstructionTokenIndexer instructionTokenIndexer,
        InstructionTokenExtractor instructionTokenExtractor,
        IEnumerable<IInstructionArgumentBuilder> instructionArgumentBuilders)
    {
        _instructionTokenIndexer = instructionTokenIndexer;
        _instructionTokenExtractor = instructionTokenExtractor;
        _instructionArgumentBuilders = instructionArgumentBuilders;
    }

    /// <inheritdoc/>
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