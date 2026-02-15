using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class StringInstructionArgumentBuilderTests
{
    private StringInstructionArgumentBuilder _stringInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _stringInstructionArgumentBuilder = new StringInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenStringArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _stringInstructionArgumentBuilder.For("hello hello hello");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        // Act
        var result = _stringInstructionArgumentBuilder.For("1");
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenFor_ShouldReturnFalse()
    {
        // Act
        var result = _stringInstructionArgumentBuilder.For(null);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenStringArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        // Act
        var result = _stringInstructionArgumentBuilder.Create(string.Empty, "test test test");

        // Assert
        var typed = result as InstructionArgument<string>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.EqualTo("test test test"));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        // Assert
        Assert.That<AnonymousInstructionArgument>((
            ) => _stringInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}