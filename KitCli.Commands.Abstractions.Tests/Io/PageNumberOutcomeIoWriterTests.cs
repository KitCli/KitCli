using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class PageNumberOutcomeIoWriterTests
{
    [Test]
    public void GivenPageNumberOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new PageNumberOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new PageNumberOutcome(2)), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new PageNumberOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenPageNumberOutcome_WhenWrite_ThenSaysItsPageNumber()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new PageNumberOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new PageNumberOutcome(2));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "Page Number: 2" }).AsCollection);
    }
}
