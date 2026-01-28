using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class DecimalCliInstructionArgumentBuilderTests
{
    private DecimalCliInstructionArgumentBuilder _decimalCliInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _decimalCliInstructionArgumentBuilder = new DecimalCliInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenDecimalArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _decimalCliInstructionArgumentBuilder.For("123.45");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _decimalCliInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenDecimalArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _decimalCliInstructionArgumentBuilder.Create(string.Empty, "678.90");

        var typed = result as ValuedCliInstructionArgument<decimal>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.EqualTo(678.90m));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That<CliInstructionArgument>((
            ) => _decimalCliInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}