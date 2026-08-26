using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Instructions.Abstractions;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class BasicCreationCliCommandFactoryTests
{
    private TestCreationCliCommandFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestCreationCliCommandFactory();
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
    public void GivenAttachedInstruction_WhenCommandIsCreated_ThenTheFactorysOwnCreationLogicRuns()
    {
        // Arrange
        _factory.Attach(new Instruction("/", "test", "page", []), []);

        // Act
        var command = _factory.Create();

        // Assert
        Assert.That(command, Is.EqualTo(new TestParameterisedNextCliCommand("page")));
    }
}
