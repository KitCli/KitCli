using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes.Final;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Handlers;

[TestFixture]
public class NoCliCommandHandlerTests
{
    private record TestCliCommand : CliCommand;
    
    private class TestCliCommandHandler : CliCommandHandler<TestCliCommand>;
    
    private CliCommandHandler<TestCliCommand> _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        _classUnderTest = new TestCliCommandHandler();
    }
    
    [Test]
    public async Task GivenCommand_WhenHandle_ReturnsOutputOutcome()
    {
        // Arrange
        var command = new TestCliCommand();
        
        // Act
        var outcome = await _classUnderTest.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(outcome.Length, Is.EqualTo(1));
        
        var outputOutcome = outcome[0] as FinalSayOutcome;
        Assert.That(outputOutcome, Is.Not.Null);
        Assert.That(outputOutcome.Something, Is.Not.Null);
    }
}