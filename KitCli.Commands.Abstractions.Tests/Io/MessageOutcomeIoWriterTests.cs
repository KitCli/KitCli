using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class MessageOutcomeIoWriterTests
{
    [Test]
    public void GivenSayOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new MessageOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new SayOutcome("hello")), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new MessageOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenSayOutcome_WhenWrite_ThenSaysItsMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new MessageOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new SayOutcome("hello"));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "hello" }).AsCollection);
    }
}
