using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class TableOutcomeIoWriterTests
{
    [Test]
    public void GivenTableOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new TableOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new TableOutcome(new Table())), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new TableOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenTableOutcome_WhenWrite_ThenSaysItsRenderedTable()
    {
        // Arrange
        var table = new Table(["Name"], [["First"]]);
        var cliIo = new TestCliIo();
        var writer = new TableOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new TableOutcome(table));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { table.ToString() }).AsCollection);
    }
}
