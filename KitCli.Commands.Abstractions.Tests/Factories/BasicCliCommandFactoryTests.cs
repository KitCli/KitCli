using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Instructions.Abstractions;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class BasicCliCommandFactoryTests
{
    private BasicCliCommandFactory<TestNextCliCommand> _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new BasicCliCommandFactory<TestNextCliCommand>();
    }

    [Test]
    public void GivenUnattachedFactory_WhenCanCreateWhenIsCalled_ThenItApplies()
    {
        // Act
        var canCreate = _factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.True);
    }

    [Test]
    public void GivenAnyInstruction_WhenCanCreateWhenIsCalled_ThenItApplies()
    {
        // Arrange
        _factory.Attach(new Instruction("/", "test", "page", []), []);

        // Act
        var canCreate = _factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.True);
    }

    [Test]
    public void GivenFactory_WhenCommandIsCreated_ThenItIsTheDeclaredCommandType()
    {
        // Act
        var command = _factory.Create();

        // Assert
        Assert.That(command, Is.InstanceOf<TestNextCliCommand>());
    }

    [Test]
    public void GivenFactory_WhenCommandIsCreatedTwice_ThenEachCallCreatesANewInstance()
    {
        // Act
        var command = _factory.Create();
        var anotherCommand = _factory.Create();

        // Assert
        Assert.That(command, Is.Not.SameAs(anotherCommand));
    }
}
