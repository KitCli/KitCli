using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class PageSizeOutcomeIoWriterTests
{
    [Test]
    public void GivenPageSizeOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new PageSizeOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new PageSizeOutcome(25)), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new PageSizeOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenPageSizeOutcome_WhenWrite_ThenSaysItsPageSize()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new PageSizeOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new PageSizeOutcome(25));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "Page Size: 25" }).AsCollection);
    }
}
