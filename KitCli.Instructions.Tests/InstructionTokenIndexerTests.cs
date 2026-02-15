using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Indexers;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace KitCli.Instructions.Tests;

[TestFixture]
public class InstructionTokenIndexerTests
{
    private IOptions<InstructionSettings> _instructionOptions;
    private InstructionTokenIndexer _instructionTokenIndexer;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _instructionOptions = Options.Create(new InstructionSettings());
        _instructionTokenIndexer = new InstructionTokenIndexer(_instructionOptions);
    }

    [Test]
    public void GivenNoInputString_WhenIndex_ReturnsFalseFlagsForAllIndexes()
    {
        var input = string.Empty;
        
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Prefix].Found, Is.False);
        Assert.That(result[InstructionTokenType.Name].Found, Is.False);
        Assert.That(result[InstructionTokenType.SubName].Found, Is.False);
        Assert.That(result[InstructionTokenType.Arguments].Found, Is.False);
    }
    
    [TestCase("command")]
    [TestCase("command-example")]
    [TestCase("command-example sub-command-example")]
    [TestCase("command-example sub-command-example --argument-example")]
    [TestCase("command-example sub-command-example --argument-example hello world")]
    [TestCase("command-example sub-command-example --argument-example hello world --argument-two")]
    [TestCase("command-example sub-command-example --argument-example hello world --argument-two 1")]
    public void GivenInputStringWithNoPrefix_WhenIndex_ReturnsFalseForFlag(string input)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Prefix].Found, Is.False);
    }

    [TestCase("/")]
    [TestCase("/command")]
    [TestCase("/command-example")]
    [TestCase("/command-example sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForCommandPrefixToken(string input)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Prefix].Found, Is.True);
        Assert.That(result[InstructionTokenType.Prefix].StartIndex, Is.EqualTo(0));
        Assert.That(result[InstructionTokenType.Prefix].EndIndex, Is.EqualTo(1));
    }

    [TestCase("/ ")]
    [TestCase("/ sub-command-example")]
    [TestCase("/ sub-command-example --argument-example")]
    [TestCase("/ sub-command-example --argument-example hello world")]
    [TestCase("/ sub-command-example --argument-example hello world --argument-two")]
    [TestCase("/ sub-command-example --argument-example hello world --argument-two 1")]
    public void GivenInputStringWithNoCommandNameToken_WhenIndex_ReturnsFalseForCommandNameTokenFlag(string input)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Name].Found, Is.False);
    }

    [TestCase("/command-example", "command-example")]
    [TestCase("/command-example sub-command-example", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "command-example")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForCommandNameToken(string input, string expectedMatch)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Name].Found, Is.True);

        var tokenIndex = result[InstructionTokenType.Name];
        var match = input[tokenIndex.StartIndex..tokenIndex.EndIndex];
        Assert.That(match, Is.EqualTo(expectedMatch));
    }
    
    [TestCase("/command-example")]
    [TestCase("/command-example --argument-example")]
    [TestCase("/command-example --argument-example hello world")]
    [TestCase("/command-example --argument-example hello world --argument-two")]
    [TestCase("/command-example --argument-example hello world --argument-two 1")]
    public void GivenInputStringWithNoSubCommandNameToken_WhenIndex_ReturnsFalseForSubCommandNameTokenFlag(string input)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.SubName].Found, Is.False);
    }
    
    [TestCase("/command-example sub-command-example", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "sub-command-example")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForSubCommandNameToken(string input, string expectedMatch)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.SubName].Found, Is.True);

        var tokenIndex = result[InstructionTokenType.SubName];
        var match = input[tokenIndex.StartIndex..tokenIndex.EndIndex];
        Assert.That(match, Is.EqualTo(expectedMatch));
    }
    
    [TestCase("/command-example")]
    [TestCase("/command-example sub-command-example")]
    public void GivenInputStringWithNoArgumentTokens_WhenIndex_ReturnsFalseForArgumentTokensIndexed(string input)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Arguments].Found, Is.False);
    }
    
    [TestCase("/command-example sub-command-example --argument-example", "--argument-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "--argument-example hello world")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "--argument-example hello world --argument-two")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "--argument-example hello world --argument-two 1")]
    [TestCase("/command-example sub-command-example --argument-example true --argument-two 1", "--argument-example true --argument-two 1")]
    [TestCase("/command-example sub-command-example --argument-example --argument-two 1", "--argument-example --argument-two 1")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForArgumentTokens(string input, string expectedMatch)
    {
        var result = _instructionTokenIndexer.Index(input);
        
        Assert.That(result[InstructionTokenType.Arguments].Found, Is.True);

        var tokenIndex = result[InstructionTokenType.Arguments];
        var match = input[tokenIndex.StartIndex..tokenIndex.EndIndex];
        Assert.That(match, Is.EqualTo(expectedMatch));
    }
}