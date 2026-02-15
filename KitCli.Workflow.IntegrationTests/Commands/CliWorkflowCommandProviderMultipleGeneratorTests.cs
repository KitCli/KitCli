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
public class CliWorkflowCommandProviderMultipleGeneratorTests
{
    private record TestCliCommand(int Number) : CliCommand;
    
    private class TestCliCommandGeneratorA : CliCommandFactory<TestCliCommand>
    {
        public override bool CanCreateWhen() => SubCommandIs("1");

        public override CliCommand Create() => new TestCliCommand(1);
    }
    
    private class TestCliCommandGeneratorB : CliCommandFactory<TestCliCommand>
    {
        public override bool CanCreateWhen() => SubCommandIs("2");
        
        public override CliCommand Create() => new TestCliCommand(2);
    }
    
    private IServiceCollection _serviceCollection;
    private ServiceProvider _serviceProvider;
    private CliWorkflowCommandProvider _cliWorkflowCommandProvider;
    
    private TestCliCommand _testCliCommand;
    private TestCliCommandGeneratorA _testCliCommandGeneratorA;
    private TestCliCommandGeneratorB _testCliCommandGeneratorB;
    
    [SetUp]
    public void SetUp()
    {
        _testCliCommand = new TestCliCommand(0);
        _testCliCommandGeneratorA = new TestCliCommandGeneratorA();
        _testCliCommandGeneratorB = new TestCliCommandGeneratorB();
        
        var serviceKey = _testCliCommand.GetInstructionName();
        _serviceCollection = new ServiceCollection();
        _serviceCollection
            .AddKeyedSingleton<ICliCommandFactory>(
                serviceKey,
                _testCliCommandGeneratorA)
            .AddKeyedSingleton<ICliCommandFactory>(
                serviceKey,
                _testCliCommandGeneratorB);
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        
        _cliWorkflowCommandProvider = new CliWorkflowCommandProvider(_serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    [TestCase("1", 1)]
    [TestCase("2", 2)]
    public void GivenSubCommand_WhenGetCommand_ThenReturnsExpectedCommandInstance(string subCommandName, int expectedNumber)
    {
        // Arrange
        var instruction = new Instruction("/", "test", subCommandName, []);
        var outcomes = new List<Outcome>();
        
        // Act
        var cliCommand = _cliWorkflowCommandProvider.GetCommand(instruction, outcomes);
        
        // Assert
        var testCliCommand = cliCommand as TestCliCommand;
        
        Assert.That(testCliCommand, Is.Not.Null);
        Assert.That(testCliCommand.Number, Is.EqualTo(expectedNumber));
    }
}