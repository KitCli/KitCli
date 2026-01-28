using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class StringCliInstructionArgumentBuilderTests
{
    private StringCliInstructionArgumentBuilder _stringCliInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _stringCliInstructionArgumentBuilder = new StringCliInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenStringArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _stringCliInstructionArgumentBuilder.For("hello hello hello");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        // Act
        var result = _stringCliInstructionArgumentBuilder.For("1");
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenFor_ShouldReturnFalse()
    {
        // Act
        var result = _stringCliInstructionArgumentBuilder.For(null);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenStringArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        // Act
        var result = _stringCliInstructionArgumentBuilder.Create(string.Empty, "test test test");

        // Assert
        var typed = result as ValuedCliInstructionArgument<string>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.EqualTo("test test test"));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        // Assert
        Assert.That<CliInstructionArgument>((
            ) => _stringCliInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}