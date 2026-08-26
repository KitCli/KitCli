using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Instructions.Abstractions;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class BasicDecisionCliCommandFactoryTests
{
    private TestDecisionCliCommandFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestDecisionCliCommandFactory();
    }

    [Test]
    public void GivenTheSubCommandItAppliesTo_WhenCanCreateWhenIsCalled_ThenItApplies()
    {
        // Arrange
        _factory.Attach(InstructionWith(TestDecisionCliCommandFactory.AppliesToSubCommandName), []);

        // Act
        var canCreate = _factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.True);
    }

    [Test]
    public void GivenAnotherSubCommand_WhenCanCreateWhenIsCalled_ThenItDoesNotApply()
    {
        // Arrange
        _factory.Attach(InstructionWith("page"), []);

        // Act
        var canCreate = _factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.False);
    }

    [Test]
    public void GivenFactory_WhenCommandIsCreated_ThenItIsTheDeclaredCommandType()
    {
        // Act
        var command = _factory.Create();

        // Assert
        Assert.That(command, Is.InstanceOf<TestVariantNextCliCommand>());
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

    private static Instruction InstructionWith(string subInstructionName)
        => new("/", "test", subInstructionName, []);
}
