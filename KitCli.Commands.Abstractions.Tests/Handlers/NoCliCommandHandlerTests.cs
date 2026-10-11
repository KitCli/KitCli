using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes.Final;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Handlers;

[TestFixture]
public class NoCliCommandHandlerTests
{
    private record NoHandlerCliCommand : CliCommand;

    private class NoHandlerCliCommandHandler : CliCommandHandler<NoHandlerCliCommand>;

    private CliCommandHandler<NoHandlerCliCommand> _classUnderTest;

    [SetUp]
    public void SetUp()
    {
        _classUnderTest = new NoHandlerCliCommandHandler();
    }

    [Test]
    public async Task GivenCommand_WhenHandle_ReturnsOutputOutcome()
    {
        // Arrange
        var command = new NoHandlerCliCommand();
        
        // Act
        var outcome = await _classUnderTest.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(outcome.Length, Is.EqualTo(1));
        
        var outputOutcome = outcome[0] as FinalSayOutcome;
        Assert.That(outputOutcome, Is.Not.Null);
        Assert.That(outputOutcome.Something, Is.Not.Null);
    }
}