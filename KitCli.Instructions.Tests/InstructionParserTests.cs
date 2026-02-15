using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;
using KitCli.Instructions.Parsers;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace KitCli.Instructions.Tests;

[TestFixture]
public class InstructionParserTests
{
    private IOptions<InstructionSettings> _instructionOptions;
    private InstructionTokenIndexer _instructionTokenIndexer;
    private InstructionTokenExtractor _instructionTokenExtractor;
    private IEnumerable<IInstructionArgumentBuilder> _instructionArgumentBuilders;
    private InstructionParser _parser;

    [SetUp]
    public void SetUp()
    {
        _instructionOptions = Options.Create(new InstructionSettings());
        
        _instructionTokenIndexer = new InstructionTokenIndexer(_instructionOptions);
        
        _instructionTokenExtractor = new InstructionTokenExtractor();
        
        _instructionArgumentBuilders = new List<IInstructionArgumentBuilder>
        {
            new StringInstructionArgumentBuilder(),
            new IntInstructionArgumentBuilder(),
        };

        _parser = new InstructionParser(
            _instructionTokenIndexer,
            _instructionTokenExtractor,
            _instructionArgumentBuilders);
    }

    [Test]
    public void GivenParserTokensWithPrefix_WhenParse_ThenReturnsInstructionWithPrefix()
    {
        var result = _parser.Parse("/name");
        
        Assert.That(result.Prefix, Is.EqualTo("/"));
    }

    [Test]
    public void GivenParserTokensWithName_WhenParse_ThenReturnsInstructionWithName()
    {
        var result = _parser.Parse("/name");
        
        Assert.That(result.Name, Is.EqualTo("name"));
    }

    [Test]
    public void GivenExtractionWithSubNae_WhenParse_ThenReturnsInstructionWithSubNae()
    {
        var result = _parser.Parse("/name subname");
        
        Assert.That(result.SubInstructionName, Is.EqualTo("subname"));
    }

    [Test]
    public void GivenParserWithStringArguments_WhenParse_ThenReturnsInstructionWithStringTypedArguments()
    {
        var result = _parser.Parse("/command --argument-one hello world");

        var argument = result.Arguments
            .OfType<InstructionArgument<string>>()
            .FirstOrDefault();
        
        Assert.That(argument, Is.Not.Null);
    }
    
    
    [Test]
    public void GivenParserWithIntArguments_WhenParse_ThenReturnsInstructionWithIntTypedArguments()
    {
        var result = _parser.Parse("/name --argument-one 1");

        var argument = result.Arguments
            .OfType<InstructionArgument<int>>()
            .FirstOrDefault();
        
        Assert.That(argument, Is.Not.Null);
    }
}