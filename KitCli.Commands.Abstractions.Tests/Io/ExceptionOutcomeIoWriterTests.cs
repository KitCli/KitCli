using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class ExceptionOutcomeIoWriterTests
{
    [Test]
    public void GivenExceptionOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new ExceptionOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new ExceptionOutcome(new Exception("boom"))), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new ExceptionOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenExceptionOutcome_WhenWrite_ThenSaysItsExceptionMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new ExceptionOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new ExceptionOutcome(new Exception("boom")));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "boom" }).AsCollection);
    }
}
