using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class BoolInstructionArgumentBuilderTests
{
    private BoolInstructionArgumentBuilder _boolInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _boolInstructionArgumentBuilder = new BoolInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenBoolArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _boolInstructionArgumentBuilder.For("true");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _boolInstructionArgumentBuilder.For("hello test");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenBoolArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        // Act
        var result = _boolInstructionArgumentBuilder.Create(string.Empty, "false");

        // Assert
        var typed = result as InstructionArgument<bool>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.False);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenCreate_ShouldReturnInstructionArgumentWithDefaultValue()
    {
        // Act
        var result = _boolInstructionArgumentBuilder.Create(string.Empty, "hello test");

        // Assert
        var typed = result as InstructionArgument<bool>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.True);
    }
}