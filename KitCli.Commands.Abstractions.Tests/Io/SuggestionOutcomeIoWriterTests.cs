using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class SuggestionOutcomeIoWriterTests
{
    [Test]
    public void GivenSuggestionOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new SuggestionOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new SuggestionOutcome("/list", "Lists things")), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new SuggestionOutcomeIoWriter(new TestCliIo());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenSuggestionOutcome_WhenWrite_ThenPausesThenSaysNameAndDescription()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new SuggestionOutcomeIoWriter(cliIo);

        // Act
        writer.Write(new SuggestionOutcome("/list", "Lists things"));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { string.Empty, "/list", "Lists things" }).AsCollection);
    }
}
