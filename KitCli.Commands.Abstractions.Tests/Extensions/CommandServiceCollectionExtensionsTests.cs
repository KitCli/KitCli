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
}
