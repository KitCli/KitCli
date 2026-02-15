using KitCli.Instructions.Abstractions;
using KitCli.Instructions.Arguments;
using KitCli.Instructions.Builders;
using NUnit.Framework;

namespace KitCli.Instructions.Tests.InstructionArgumentBuilders;

[TestFixture]
public class GuidInstructionArgumentBuilderTests
{
    private GuidInstructionArgumentBuilder _guidInstructionArgumentBuilder;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _guidInstructionArgumentBuilder = new GuidInstructionArgumentBuilder();
    }
    
    [Test]
    public void GivenGuidArgumentValue_WhenFor_ShouldReturnTrue()
    {
        var result = _guidInstructionArgumentBuilder.For("d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenWrongArgumentValue_WhenFor_ShouldReturnFalse()
    {
        var result = _guidInstructionArgumentBuilder.For("hello test");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void GivenGuidArgumentValue_WhenCreate_ShouldReturnInstructionArgument()
    {
        var result = _guidInstructionArgumentBuilder.Create(string.Empty, "d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c");

        var typed = result as InstructionArgument<Guid>;
        
        Assert.That(typed, Is.Not.Null);
        Assert.That(typed.Value, Is.EqualTo(Guid.Parse("d3b07384-d9a1-4c2a-8f3d-1c3e5f6a7b8c")));
    }
    
    [Test]
    public void GivenNoArgumentValue_WhenCreate_ShouldThrowArgumentException()
    {
        Assert.That<AnonymousInstructionArgument>((
            ) => _guidInstructionArgumentBuilder.Create(string.Empty, null),
            Throws.ArgumentNullException);
    }
}