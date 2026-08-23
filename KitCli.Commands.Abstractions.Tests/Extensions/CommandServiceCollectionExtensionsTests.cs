using System.Reflection;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Factories;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Extensions;

[TestFixture]
public class CommandServiceCollectionExtensionsTests
{
    // IServiceCollection is just IList<ServiceDescriptor> - this avoids pulling in the full
    // Microsoft.Extensions.DependencyInjection package for a container we never build.
    private sealed class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection;

    private static bool IsRegistered(IServiceCollection services, object key)
        => services.Any(descriptor =>
            descriptor.ServiceType == typeof(ICliCommandFactory) &&
            Equals(descriptor.ServiceKey, key));

    [CliCommandAlias("gimme")]
    [CliCommandAlias("give-me-cash")]
    private record AliasedCommand : CliCommand;

    [Test]
    public void GivenCommandWithCliCommandAliasAttributes_WhenAddCommandsFromAssembly_ThenFactoryIsResolvableByEachAlias()
    {
        // Arrange
        var services = new TestServiceCollection();

        // Act
        services.AddCommandsFromAssembly(Assembly.GetExecutingAssembly());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(IsRegistered(services, "gimme"), Is.True);
            Assert.That(IsRegistered(services, "give-me-cash"), Is.True);
        });
    }
}
