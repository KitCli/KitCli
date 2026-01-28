using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class BoolCliInstructionArgumentBuilderTests
{
    private BoolCliInstructionArgumentBuilder _boolCliInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _boolCliInstructionArgumentBuilder = new BoolCliInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenBoolArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _boolCliInstructionArgumentBuilder.For("true");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnTrue()
    {
        // Act
        var result = _boolCliInstructionArgumentBuilder.For("hello test");
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenBoolArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        // Act
        var result = _boolCliInstructionArgumentBuilder.Create(string.Empty, "false");

        // Assert
        var typed = result as ValuedCliInstructionArgument<bool>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.False);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenCreate_ShouldReturnInstructionArgumentWithDefaultValue()
    {
        // Act
        var result = _boolCliInstructionArgumentBuilder.Create(string.Empty, "hello test");

        // Assert
        var typed = result as ValuedCliInstructionArgument<bool>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.True);
    }
}