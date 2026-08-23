using System.Reflection;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Factories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Extensions;

[TestFixture]
public class CommandServiceCollectionExtensionsTests
{
    [CliCommandAlias("gimme")]
    [CliCommandAlias("give-me-cash")]
    private record AliasedCommand : CliCommand;

    [Test]
    public void GivenCommandWithCliCommandAliasAttributes_WhenAddCommandsFromAssembly_ThenFactoryIsResolvableByEachAlias()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(provider.GetKeyedService<ICliCommandFactory>("gimme"), Is.Not.Null);
            Assert.That(provider.GetKeyedService<ICliCommandFactory>("give-me-cash"), Is.Not.Null);
        });
    }

    private record RegistrationExampleCliCommand : CliCommand;

    [Test]
    public void GivenArgumentFreeCommand_WhenAddCommandsFromAssembly_ThenFactoryIsResolvableByFullAndShorthandName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(provider.GetKeyedService<ICliCommandFactory>("registration-example"), Is.Not.Null);
            Assert.That(provider.GetKeyedService<ICliCommandFactory>("re"), Is.Not.Null);
        });
    }

    private record DedicatedFactoryCliCommand : CliCommand;

    private class DedicatedFactoryCliCommandFactory : CliCommandFactory<DedicatedFactoryCliCommand>
    {
        public override bool CanCreateWhen() => true;
        public override CliCommand Create() => new DedicatedFactoryCliCommand();
    }

    [Test]
    public void GivenCommandWithDedicatedFactory_WhenAddCommandsFromAssembly_ThenDedicatedFactoryIsUsedInsteadOfBasicFallback()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();
        var factory = provider.GetKeyedService<ICliCommandFactory>("dedicated-factory");

        // Assert
        Assert.That(factory, Is.TypeOf<DedicatedFactoryCliCommandFactory>());
    }

    private record NoFactoryCliCommand(string Text) : CliCommand;

    [Test]
    public void GivenCommandWithNoParameterlessConstructorAndNoDedicatedFactory_WhenAddCommandsFromAssembly_ThenNoFactoryIsRegisteredForIt()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.That(provider.GetKeyedService<ICliCommandFactory>("no-factory"), Is.Null);
    }

    [Test]
    public void GivenNullAssembly_WhenAddCommandsFromAssembly_ThenThrowsNullReferenceException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => services.AddCommandsFromAssembly(null));
    }

    [Test]
    public void GivenAssemblyWithNoCliCommandImplementations_WhenAddCommandsFromAssembly_ThenThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var assemblyWithNoCommands = typeof(TestFixtureAttribute).Assembly;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddCommandsFromAssembly(assemblyWithNoCommands));
    }
}
