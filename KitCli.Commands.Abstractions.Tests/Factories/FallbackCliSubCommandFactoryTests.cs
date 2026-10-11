using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using KitCli.Instructions.Abstractions;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class FallbackCliSubCommandFactoryTests
{
    [Test]
    public void GivenNoSubCommand_WhenCanCreateWhenForThePrimaryCommandItself_ThenItApplies()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryCliCommand>();
        factory.Attach(InstructionWith(null), []);

        // Act
        var canCreate = factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.True);
    }

    [Test]
    public void GivenASubCommand_WhenCanCreateWhenForThePrimaryCommandItself_ThenItDoesNotApply()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryCliCommand>();
        factory.Attach(InstructionWith("uncovered-sub"), []);

        // Act
        var canCreate = factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.False);
    }

    [Test]
    public void GivenNoSubCommand_WhenCanCreateWhenForASibling_ThenItDoesNotApply()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryUncoveredSubCliCommand>();
        factory.Attach(InstructionWith(null), []);

        // Act
        var canCreate = factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.False);
    }

    [Test]
    public void GivenTheSiblingsOwnSubCommandWord_WhenCanCreateWhenForThatSibling_ThenItApplies()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryUncoveredSubCliCommand>();
        factory.Attach(InstructionWith("uncovered-sub"), []);

        // Act
        var canCreate = factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.True);
    }

    [Test]
    public void GivenADifferentSubCommandWord_WhenCanCreateWhenForASibling_ThenItDoesNotApply()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryUncoveredSubCliCommand>();
        factory.Attach(InstructionWith("dedicated-sub"), []);

        // Act
        var canCreate = factory.CanCreateWhen();

        // Assert
        Assert.That(canCreate, Is.False);
    }

    [Test]
    public void WhenCommandIsCreated_ThenItIsANewInstanceOfTheDeclaredSiblingType()
    {
        // Arrange
        var factory = new FallbackCliSubCommandFactory<TestPrimaryCliCommand, TestPrimaryUncoveredSubCliCommand>();

        // Act
        var command = factory.Create();

        // Assert
        Assert.That(command, Is.InstanceOf<TestPrimaryUncoveredSubCliCommand>());
    }

    private static Instruction InstructionWith(string? subInstructionName)
        => new("/", "test-primary", subInstructionName, []);
}
