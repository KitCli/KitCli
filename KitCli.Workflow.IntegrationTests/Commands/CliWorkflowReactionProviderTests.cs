using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Workflow.IntegrationTests.Commands;

[TestFixture]
public class CliWorkflowReactionProviderTests
{
    private record TestCliCommandReaction : CliCommandReaction;

    private record TestUnregisteredCliCommandReaction : CliCommandReaction;

    private class TestCliCommandReactionGenerator : BasicCliCommandReactionFactory<TestCliCommandReaction>;

    private IServiceCollection _serviceCollection;
    private ServiceProvider _serviceProvider;
    private CliWorkflowReactionProvider _cliWorkflowReactionProvider;

    private TestCliCommandReactionGenerator _cliCommandReactionGenerator;

    [SetUp]
    public void SetUp()
    {
        _cliCommandReactionGenerator = new TestCliCommandReactionGenerator();

        _serviceCollection = new ServiceCollection();
        _serviceCollection
            .AddKeyedSingleton<ICliCommandReactionFactory>(
                typeof(TestCliCommandReaction),
                _cliCommandReactionGenerator);

        _serviceProvider = _serviceCollection.BuildServiceProvider();

        _cliWorkflowReactionProvider = new CliWorkflowReactionProvider(_serviceProvider);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void GivenReactionWithSingleFactory_WhenGetReaction_ThenReturnsExpectedReactionInstance()
    {
        // Arrange
        var outcomes = new List<Outcome>();

        // Act
        var result = _cliWorkflowReactionProvider.GetReaction(typeof(TestCliCommandReaction), outcomes);

        // Assert
        Assert.That(result, Is.InstanceOf<TestCliCommandReaction>());
    }

    [Test]
    public void GivenReactionWithNoFactory_WhenGetReaction_ThenThrowsNoReactionFactoryException()
    {
        // Arrange
        var outcomes = new List<Outcome>();

        // Act & Assert
        Assert.Throws<NoReactionFactoryException>(() =>
            _cliWorkflowReactionProvider.GetReaction(typeof(TestUnregisteredCliCommandReaction), outcomes));
    }
}
