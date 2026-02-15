using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class DecimalInstructionArgumentBuilderTests
{
    private DecimalInstructionArgumentBuilder _decimalInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _decimalInstructionArgumentBuilder = new DecimalInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenDecimalArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _decimalInstructionArgumentBuilder.For("123.45");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _decimalInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenDecimalArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _decimalInstructionArgumentBuilder.Create(string.Empty, "678.90");

        var typed = result as InstructionArgument<decimal>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.EqualTo(678.90m));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That<AnonymousInstructionArgument>((
            ) => _decimalInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}