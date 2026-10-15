using KitCli.Abstractions.Io;
using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
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
        var writer = new MessageOutcomeIoWriter(new TestCliIo(), new KitCliSettings());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new SayOutcome("hello")), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new MessageOutcomeIoWriter(new TestCliIo(), new KitCliSettings());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenSayOutcome_WhenWrite_ThenSaysItsMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new MessageOutcomeIoWriter(cliIo, new KitCliSettings());

        // Act
        writer.Write(new SayOutcome("hello"));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "hello" }).AsCollection);
    }

    [Test]
    public void GivenLeveledSayOutcomeAtMinimumLevel_WhenWrite_ThenSaysItsMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new MessageOutcomeIoWriter(cliIo, new KitCliSettings { MinimumMessageLevel = MessageLevel.Warning });

        // Act
        writer.Write(new LeveledSayOutcome("careful", MessageLevel.Warning));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "careful" }).AsCollection);
    }

    [Test]
    public void GivenLeveledSayOutcomeBelowMinimumLevel_WhenWrite_ThenSaysNothing()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new MessageOutcomeIoWriter(cliIo, new KitCliSettings { MinimumMessageLevel = MessageLevel.Warning });

        // Act
        writer.Write(new LeveledSayOutcome("quiet", MessageLevel.Debug));

        // Assert
        Assert.That(cliIo.Lines, Is.Empty);
    }
}
