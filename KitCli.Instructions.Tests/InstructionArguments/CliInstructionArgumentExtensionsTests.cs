using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArguments;

// TODO: I think we can create a generic Test Case for this.
[TestFixture]
public class CliInstructionArgumentExtensionsTests
{
    private List<CliInstructionArgument> _arguments;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var dateOnly = new DateTime(1, 1, 1);
        
        _arguments =
        [
            new ValuedCliInstructionArgument<string>("argumentOne", "hello"),
            new ValuedCliInstructionArgument<int>("argumentTwo", 12345),
            new ValuedCliInstructionArgument<bool>("argumentThree", true),
            new ValuedCliInstructionArgument<decimal>("argumentFour", 12.0m),
            new ValuedCliInstructionArgument<DateOnly>("argumentFive", DateOnly.FromDateTime(dateOnly))
        ];
    }
    
    [Test]
    public void GivenStringArgumentWithName_WhenOfType_ReturnsArgumentOfType()
    {
        var result = _arguments.OfType<string>("argumentOne");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ArgumentValue, Is.EqualTo("hello"));
    }
    
    [Test]
    public void GivenIntArgumentWithName_WhenOfType_ReturnsArgumentOfType()
    {
        var result = _arguments.OfType<int>("argumentTwo");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ArgumentValue, Is.EqualTo(12345));
    }
    
    [Test]
    public void GivenBoolArgumentWithName_WhenOfType_ReturnsArgumentOfType()
    {
        var result = _arguments.OfType<bool>("argumentThree");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ArgumentValue, Is.EqualTo(true));
    }
    
    [Test]
    public void GivenFourArgumentWithName_WhenOfType_ReturnsArgumentOfType()
    {
        var result = _arguments.OfType<decimal>("argumentFour");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ArgumentValue, Is.EqualTo(12.0m));
    }

    [Test]
    public void GivenDateOnlyArgumentWithName_WhenOfType_ReturnsArgumentOfType()
    {
        var result = _arguments.OfType<DateOnly>("argumentFive");

        var expectedDate = new DateTime(1, 1, 1);
        var expectedDateTime = DateOnly.FromDateTime(expectedDate);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ArgumentValue, Is.EqualTo(expectedDateTime));
    }
    
    [Test]
    public void GivenNoArgumentWithSameName_WhenOfType_ReturnsNull()
    {
        var result = _arguments.OfType<int>("argumentFive");
        
        Assert.That(result, Is.Null);
    }
    

}