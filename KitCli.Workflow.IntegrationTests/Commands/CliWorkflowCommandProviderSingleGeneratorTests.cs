using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Workflow.IntegrationTests.Commands;

[TestFixture]
public class CliWorkflowCommandProviderSingleGeneratorTests
{
    private record TestCliCommand : CliCommand;

    private class TestCliCommandGenerator : BasicCliCommandFactory<TestCliCommand>;
    
    private IServiceCollection _serviceCollection;
    private ServiceProvider _serviceProvider;
    private CliWorkflowCommandProvider _cliWorkflowCommandProvider;
    
    private TestCliCommand _cliCommand;
    private TestCliCommandGenerator _cliCommandGenerator;
    
    [SetUp]
    public void SetUp()
    {
        _cliCommand = new TestCliCommand();
        _cliCommandGenerator = new TestCliCommandGenerator();
        
        _serviceCollection = new ServiceCollection();
        _serviceCollection
            .AddKeyedSingleton<IUnidentifiedCliCommandFactory>(
                _cliCommand.GetInstructionName(),
                _cliCommandGenerator);
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        
        _cliWorkflowCommandProvider = new CliWorkflowCommandProvider(_serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public void GivenCommandWithSingleGenerator_WhenGetCommand_ThenReturnsExpectedCommandInstance()
    {
        // Arrange
        var instruction = new CliInstruction("/", "test", null, []);
        var outcomes = new List<Outcome>();
        
        // Act
        var result = _cliWorkflowCommandProvider.GetCommand(instruction, outcomes);
        
        // Assert
        Assert.That(result, Is.InstanceOf<TestCliCommand>());
    }
}