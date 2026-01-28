using KitCli.Instructions.Extraction;
using KitCli.Instructions.Indexers;
using NUnit.Framework;

namespace KitCli.Instructions.Tests;

[TestFixture]
public class CliInstructionTokenExtractorTests
{
    private CliInstructionTokenExtractor _legacyCliInstructionTokenExtractor;

    [SetUp]
    public void SetUp()
    {
        _legacyCliInstructionTokenExtractor = new CliInstructionTokenExtractor();
    }
    
    [Test]
    public void GivenInputString_WhenParse_ReturnsInstructionPrefixs()
    {
        var input = $"/command";

        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = false
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        Assert.That(result.PrefixToken, Is.EqualTo("/"));
    }
    
    [Test]
    public void GivenInputStringWithoutArguments_WhenParse_ReturnsInstructionWithoutArguments()
    {
        var input = $"/command";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = input.Length
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = false
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        Assert.That(result.NameToken, Is.EqualTo("command"));
    }

    [Test]
    public void GivenInputStringWithSubCommand_WhenParse_ReturnsInstructionWithSubCommand()
    {
        var input = $"/command sub-command";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = false
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        Assert.That(result.SubNameToken, Is.EqualTo("sub-command"));
    }

    [Test]
    public void GivenInputWithArguments_WhenParse_ReturnsInstructionsWithCorrectName()
    {
        var input = $"/command --argumentOne hello world";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 8
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        Assert.That(result.NameToken, Is.EqualTo("command"));
    }
    
    
    [Test]
    public void GivenInputStringWithArgumentsAndNoValue_WhenParse_ReturnsInstructionWithEmptyArguments()
    {
        var argumentName = "argumentOne";
        
        var input = $"/command --{argumentName}";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        var resultingArgument = result.ArgumentTokens.First();
        
        Assert.That(resultingArgument.Key, Is.EqualTo(argumentName));
        Assert.That(resultingArgument.Value, Is.Null);
    }
    
    [Test]
    public void GivenInputString_WhenParse_ReturnsInstructionWithOneStringArguments()
    {
        var argumentName = "argumentOne";
        var argumentValue = "hello world";
        
        var input = $"/command --{argumentName} {argumentValue}";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        var resultingArgument = result.ArgumentTokens.First();
        
        Assert.That(resultingArgument.Key, Is.EqualTo(argumentName));
        Assert.That(resultingArgument.Value, Is.EqualTo(argumentValue));
    }
    
    [Test]
    public void GivenInputString_WhenParse_ReturnsInstructionWithTwoStringArguments()
    {
        var argumentOneName = "argumentOne";
        var argumentOneValue = "hello world";
        var argumentTwoName = "argumentTwo";
        var argumentTwoValue = "world hello";
        
        var input = $"/command --{argumentOneName} {argumentOneValue} --{argumentTwoName} {argumentTwoValue}";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);

        var resultingArgumentOne = result.ArgumentTokens.FirstOrDefault();
        
        Assert.That(resultingArgumentOne.Key, Is.EqualTo(argumentOneName));
        Assert.That(resultingArgumentOne.Value, Is.EqualTo(argumentOneValue));

        var resultingArgumentTwo = result.ArgumentTokens.LastOrDefault();
        
        Assert.That(resultingArgumentTwo.Key, Is.EqualTo(argumentTwoName));
        Assert.That(resultingArgumentTwo.Value, Is.EqualTo(argumentTwoValue));
    }
    
    [Test]
    public void GivenInputString_WhenParse_ReturnsInstructionWithMultipleTypedArguments()
    {
        var argumentOneName = "argumentOne";
        var argumentOneValue = "hello world";
        var argumentTwoName = "argumentTwo";
        var argumentTwoValue = "1";
        
        var input = $"/command --{argumentOneName} {argumentOneValue} --{argumentTwoName} {argumentTwoValue}";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        var resultingArgumentOne = result.ArgumentTokens.FirstOrDefault();
        
        Assert.That(resultingArgumentOne.Key, Is.EqualTo(argumentOneName));
        Assert.That(resultingArgumentOne.Value, Is.EqualTo(argumentOneValue));
        
        var resultingArgumentTwo = result.ArgumentTokens.LastOrDefault();
        
        Assert.That(resultingArgumentTwo.Key, Is.EqualTo(argumentTwoName));
        Assert.That(resultingArgumentTwo.Value, Is.EqualTo(argumentTwoValue));
    }
    
    [Test]
    public void GivenInputString_WhenParse_ReturnsInstructionWithNulLArgumentValue()
    {
        var argumentOneName = "argumentOne";
        var argumentTwoName = "argumentTwo";
        var argumentTwoValue = "1";
        
        var input = $"/command --{argumentOneName} --{argumentTwoName} {argumentTwoValue}";
        
        var indexes = new CliInstructionTokenIndexCollection
        {
            [CliInstructionTokenType.Prefix] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 0,
                EndIndex = 1
            },
            [CliInstructionTokenType.Name] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 1,
                EndIndex = 7
            },
            [CliInstructionTokenType.SubName] = new CliInstructionTokenIndex
            {
                Found = false
            },
            [CliInstructionTokenType.Arguments] = new CliInstructionTokenIndex
            {
                Found = true,
                StartIndex = 9,
                EndIndex = input.Length
            }
        };
        
        var result = _legacyCliInstructionTokenExtractor.Extract(indexes, input);
        
        var resultingArgumentOne = result.ArgumentTokens.FirstOrDefault();
        
        Assert.That(resultingArgumentOne.Key, Is.EqualTo(argumentOneName));
        Assert.That(resultingArgumentOne.Value, Is.Null);
        
        var resultingArgumentTwo = result.ArgumentTokens.LastOrDefault();
        
        Assert.That(resultingArgumentTwo.Key, Is.EqualTo(argumentTwoName));
        Assert.That(resultingArgumentTwo.Value, Is.EqualTo(argumentTwoValue));
    }
}