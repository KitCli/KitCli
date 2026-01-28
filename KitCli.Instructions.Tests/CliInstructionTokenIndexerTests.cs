using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Indexers;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace KitCli.Instructions.Tests;

[TestFixture]
public class CliInstructionTokenIndexerTests
{
    private IOptions<InstructionSettings> _instructionOptions;
    private CliInstructionTokenIndexer _cliInstructionTokenIndexer;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _instructionOptions = Options.Create(new InstructionSettings());
        _cliInstructionTokenIndexer = new CliInstructionTokenIndexer(_instructionOptions);
    }

    [Test]
    public void GivenNoInputString_WhenIndex_ReturnsFalseFlagsForAllIndexes()
    {
        var input = string.Empty;
        
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Prefix].Found, Is.False);
        Assert.That(result[CliInstructionTokenType.Name].Found, Is.False);
        Assert.That(result[CliInstructionTokenType.SubName].Found, Is.False);
        Assert.That(result[CliInstructionTokenType.Arguments].Found, Is.False);
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
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Prefix].Found, Is.False);
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
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Prefix].Found, Is.True);
        Assert.That(result[CliInstructionTokenType.Prefix].StartIndex, Is.EqualTo(0));
        Assert.That(result[CliInstructionTokenType.Prefix].EndIndex, Is.EqualTo(1));
    }

    [TestCase("/ ")]
    [TestCase("/ sub-command-example")]
    [TestCase("/ sub-command-example --argument-example")]
    [TestCase("/ sub-command-example --argument-example hello world")]
    [TestCase("/ sub-command-example --argument-example hello world --argument-two")]
    [TestCase("/ sub-command-example --argument-example hello world --argument-two 1")]
    public void GivenInputStringWithNoCommandNameToken_WhenIndex_ReturnsFalseForCommandNameTokenFlag(string input)
    {
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Name].Found, Is.False);
    }

    [TestCase("/command-example", "command-example")]
    [TestCase("/command-example sub-command-example", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "command-example")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForCommandNameToken(string input, string expectedMatch)
    {
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Name].Found, Is.True);

        var tokenIndex = result[CliInstructionTokenType.Name];
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
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.SubName].Found, Is.False);
    }
    
    [TestCase("/command-example sub-command-example", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "sub-command-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "sub-command-example")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForSubCommandNameToken(string input, string expectedMatch)
    {
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.SubName].Found, Is.True);

        var tokenIndex = result[CliInstructionTokenType.SubName];
        var match = input[tokenIndex.StartIndex..tokenIndex.EndIndex];
        Assert.That(match, Is.EqualTo(expectedMatch));
    }
    
    [TestCase("/command-example")]
    [TestCase("/command-example sub-command-example")]
    public void GivenInputStringWithNoArgumentTokens_WhenIndex_ReturnsFalseForArgumentTokensIndexed(string input)
    {
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Arguments].Found, Is.False);
    }
    
    [TestCase("/command-example sub-command-example --argument-example", "--argument-example")]
    [TestCase("/command-example sub-command-example --argument-example hello world", "--argument-example hello world")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two", "--argument-example hello world --argument-two")]
    [TestCase("/command-example sub-command-example --argument-example hello world --argument-two 1", "--argument-example hello world --argument-two 1")]
    [TestCase("/command-example sub-command-example --argument-example true --argument-two 1", "--argument-example true --argument-two 1")]
    [TestCase("/command-example sub-command-example --argument-example --argument-two 1", "--argument-example --argument-two 1")]
    public void GivenInputString_WhenIndex_ReturnsCorrectIndexesForArgumentTokens(string input, string expectedMatch)
    {
        var result = _cliInstructionTokenIndexer.Index(input);
        
        Assert.That(result[CliInstructionTokenType.Arguments].Found, Is.True);

        var tokenIndex = result[CliInstructionTokenType.Arguments];
        var match = input[tokenIndex.StartIndex..tokenIndex.EndIndex];
        Assert.That(match, Is.EqualTo(expectedMatch));
    }
}