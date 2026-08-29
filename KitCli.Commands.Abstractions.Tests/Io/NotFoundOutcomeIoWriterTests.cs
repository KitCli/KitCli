using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class NotFoundOutcomeIoWriterTests
{
    [Test]
    public void GivenCliCommandNotFoundOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new NotFoundOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new CliCommandNotFoundOutcome()), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new NotFoundOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenCliCommandNotFoundOutcome_WhenWrite_ThenSaysCommandNotFound()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new NotFoundOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new CliCommandNotFoundOutcome());

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "Command Not Found" }).AsCollection);
    }
}
