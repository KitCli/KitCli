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
public class CliWorkflowCommandProviderCommandArtefactTests
{
    private record TestCliCommand : CliCommand;

    private class TestOutcome() : Outcome(OutcomeKind.Reusable);
    
    private class TestCliCommandArtefact() : CliCommandArtefact("Test");
    
    private class TestCliCommandArtefactFactory : ICliCommandArtefactFactory
    {
        public bool For(Outcome outcome) => outcome is TestOutcome;

        public CliCommandArtefact Create(Outcome outcome) => new TestCliCommandArtefact();
    }

    private class TestCliCommandFactory : BasicCliCommandFactory<TestCliCommand>;
    
    private IServiceCollection _serviceCollection;
    private ServiceProvider _serviceProvider;
    private CliWorkflowCommandProvider _cliWorkflowCommandProvider;
    
    
    [SetUp]
    public void SetUp()
    {
        var serviceKey = new TestCliCommand().GetInstructionName();
        
        _serviceCollection = new ServiceCollection();
        _serviceCollection
            .AddKeyedSingleton<IUnidentifiedCliCommandFactory, TestCliCommandFactory>(serviceKey)
            .AddSingleton<ICliCommandArtefactFactory, TestCliCommandArtefactFactory>();
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        
        _cliWorkflowCommandProvider = new CliWorkflowCommandProvider(_serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public void GivenCommandAndConvertableOutcome_WhenGetCommand_ThenReturnsExpectedCommandInstance()
    {
        // Arrange
        var instruction = new CliInstruction("/", "test", null, []);
        
        var outcomes = new List<Outcome>
        {
            new TestOutcome()
        };
        
        // Act
        var result = _cliWorkflowCommandProvider.GetCommand(instruction, outcomes);
        
        // Assert
        Assert.That(result, Is.InstanceOf<TestCliCommand>());
    }
}