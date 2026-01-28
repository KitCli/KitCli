using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class DateOnlyCliInstructionArgumentBuilderTests
{
    private DateOnlyCliInstructionArgumentBuilder _dateOnlyCliInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dateOnlyCliInstructionArgumentBuilder = new DateOnlyCliInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenDateOnlyArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _dateOnlyCliInstructionArgumentBuilder.For("2023-10-15");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _dateOnlyCliInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenDateOnlyArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _dateOnlyCliInstructionArgumentBuilder.Create(string.Empty, "2023-10-15");

        var typed = result as ValuedCliInstructionArgument<DateOnly>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.EqualTo(DateOnly.Parse("2023-10-15")));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That<CliInstructionArgument>((
            ) => _dateOnlyCliInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}