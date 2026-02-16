using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class DateOnlyInstructionArgumentBuilderTests
{
    private DateOnlyInstructionArgumentBuilder _dateOnlyInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dateOnlyInstructionArgumentBuilder = new DateOnlyInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenDateOnlyArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _dateOnlyInstructionArgumentBuilder.For("2023-10-15");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _dateOnlyInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenDateOnlyArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _dateOnlyInstructionArgumentBuilder.Create(string.Empty, "2023-10-15");

        var typed = result as InstructionArgument<DateOnly>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.EqualTo(DateOnly.Parse("2023-10-15")));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That((
            ) => _dateOnlyInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}