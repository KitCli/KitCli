using System.Reflection;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;
using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Artefacts.RanCliCommand;
using KitCli.Commands.Abstractions.Artefacts.TableBuilder;
using KitCli.Commands.Abstractions.Extensions;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Extensions;

[TestFixture]
public class ArtefactServiceCollectionExtensionsTests
{
    private static IEnumerable<IArtefactFactory> FactoriesRegisteredFromThisAssembly()
    {
        var services = new ServiceCollection();

        services.AddArtefactFactoriesForAssembly(Assembly.GetExecutingAssembly());

        return services.BuildServiceProvider().GetServices<IArtefactFactory>();
    }

    [Test]
    public void GivenAnyAssembly_WhenAddArtefactFactoriesForAssembly_ThenEveryBuiltInFactoryIsRegistered()
    {
        // Act
        var factories = FactoriesRegisteredFromThisAssembly().ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(factories, Has.Some.TypeOf<RanCliCommandArtefactFactory>());
            Assert.That(factories, Has.Some.TypeOf<PageSizeArtefactFactory>());
            Assert.That(factories, Has.Some.TypeOf<PageNumberArtefactFactory>());
            Assert.That(factories, Has.Some.TypeOf<AggregatorFilterArtefactFactory>());
        });
    }

    [Test]
    public void GivenAssemblyWithCustomFactory_WhenAddArtefactFactoriesForAssembly_ThenTheCustomFactoryIsRegistered()
    {
        // Act
        var factories = FactoriesRegisteredFromThisAssembly();

        // Assert
        Assert.That(factories, Has.Some.TypeOf<TestArtefactFactory>());
    }

    [Test]
    public void GivenAssemblyWithAggregator_WhenAddArtefactFactoriesForAssembly_ThenItsFactoryIsRegisteredUnderItsGenericArguments()
    {
        // Act
        var factories = FactoriesRegisteredFromThisAssembly();

        // Assert
        Assert.That(factories, Has.Some.TypeOf<AggregatorArtefactFactory<TestAggregate, TestAggregate>>());
    }

    [Test]
    public void GivenAssemblyWithTableBuilder_WhenAddArtefactFactoriesForAssembly_ThenItsFactoryIsRegisteredUnderItsGenericArguments()
    {
        // Act
        var factories = FactoriesRegisteredFromThisAssembly();

        // Assert
        Assert.That(factories, Has.Some.TypeOf<TableBuilderArtefactFactory<TestAggregate, TestAggregate>>());
    }

    [Test]
    public void GivenNullAssembly_WhenAddArtefactFactoriesForAssembly_ThenThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddArtefactFactoriesForAssembly(null));
    }
}
