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

    [Test]
    public void GivenArgumentValueWithTheArgumentPrefixInsideAWord_WhenParse_ThenReturnsOneArgumentKeepingThePrefix()
    {
        var result = _parser.Parse("/command --flag a--b");

        var argument = result.Arguments
            .OfType<InstructionArgument<string>>()
            .Single();
        
        Assert.That(argument.Name, Is.EqualTo("flag"));
        Assert.That(argument.Value, Is.EqualTo("a--b"));
    }

    [Test]
    public void GivenArgumentValueWithTheArgumentPrefixStandingAlone_WhenParse_ThenReturnsOneArgumentKeepingThePrefix()
    {
        var result = _parser.Parse("/command --note see -- there");

        var argument = result.Arguments
            .OfType<InstructionArgument<string>>()
            .Single();
        
        Assert.That(argument.Name, Is.EqualTo("note"));
        Assert.That(argument.Value, Is.EqualTo("see -- there"));
    }

    [Test]
    public void GivenArgumentValueEndingInTheArgumentPrefix_WhenParse_ThenReturnsOneArgumentKeepingThePrefix()
    {
        var result = _parser.Parse("/command --note abc--");

        var argument = result.Arguments
            .OfType<InstructionArgument<string>>()
            .Single();
        
        Assert.That(argument.Value, Is.EqualTo("abc--"));
    }

    [Test]
    public void GivenTwoArgumentsWhereTheFirstValueHoldsTheArgumentPrefix_WhenParse_ThenReturnsBothArguments()
    {
        var result = _parser.Parse("/command --flag a--b --count 1");

        var flagArgument = result.Arguments
            .OfType<InstructionArgument<string>>()
            .Single();
        
        Assert.That(flagArgument.Name, Is.EqualTo("flag"));
        Assert.That(flagArgument.Value, Is.EqualTo("a--b"));

        var countArgument = result.Arguments
            .OfType<InstructionArgument<int>>()
            .Single();
        
        Assert.That(countArgument.Name, Is.EqualTo("count"));
        Assert.That(countArgument.Value, Is.EqualTo(1));
    }

}