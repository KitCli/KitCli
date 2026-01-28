using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class GuidCliInstructionArgumentBuilderTests
{
    private GuidCliInstructionArgumentBuilder _guidCliInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _guidCliInstructionArgumentBuilder = new GuidCliInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenGuidArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _guidCliInstructionArgumentBuilder.For("d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _guidCliInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenGuidArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _guidCliInstructionArgumentBuilder.Create(string.Empty, "d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c");

        var typed = result as ValuedCliInstructionArgument<Guid>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.ArgumentValue, Is.EqualTo(Guid.Parse("d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c")));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That<CliInstructionArgument>((
            ) => _guidCliInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}