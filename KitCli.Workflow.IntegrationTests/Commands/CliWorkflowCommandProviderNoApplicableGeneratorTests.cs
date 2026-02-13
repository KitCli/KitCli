using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Exceptions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Instructions.Abstractions;
using KitCli.Workflow.Commands;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace KitCli.Workflow.IntegrationTests.Commands;

[TestFixture]
public class CliWorkflowCommandProviderNoApplicableGeneratorTests
{
    private record TestCliCommand : CliCommand;
    
    private class TestCliCommandGeneratorA : CliCommandFactory<TestCliCommand>
    {
        public override bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts)
            => instruction.SubInstructionName == "not applicable";

        public override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
            => new TestCliCommand();
    }
    
    private class TestCliCommandGeneratorB : CliCommandFactory<TestCliCommand>
    {
        public override bool CanCreateWhen(CliInstruction instruction, List<CliCommandArtefact> artefacts)
            => instruction.SubInstructionName == "not applicable";
        
        public override CliCommand Create(CliInstruction instruction, List<CliCommandArtefact> artefacts)
            => new TestCliCommand();
    }
    
    private IServiceCollection _serviceCollection;
    private ServiceProvider _serviceProvider;
    private CliWorkflowCommandProvider _cliWorkflowCommandProvider;
    
    [SetUp]
    public void SetUp()
    {
        var serviceKey = new TestCliCommand().GetInstructionName();
        _serviceCollection = new ServiceCollection();
        _serviceCollection
            .AddKeyedSingleton<IUnidentifiedCliCommandFactory, TestCliCommandGeneratorA>(serviceKey)
            .AddKeyedSingleton<IUnidentifiedCliCommandFactory, TestCliCommandGeneratorB>(serviceKey);
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();
        
        _cliWorkflowCommandProvider = new CliWorkflowCommandProvider(_serviceProvider);
    }
    
    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }
    
    [Test]
    public void GivenCommandAndNoApplicableGenerator_WhenGetCommand_ThenThrowsInvalidOperationException()
    {
        // Arrange
        var instruction = new CliInstruction("/", "test", "non-applicable-sub-command", []);
        var outcomes = new List<Outcome>();
        
        // Act & Assert
        Assert.Throws<NoCommandGeneratorException>(() => _cliWorkflowCommandProvider.GetCommand(instruction, outcomes));
    }
}