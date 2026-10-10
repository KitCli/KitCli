using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class OutputOutcomeIoWriterTests
{
    [Test]
    public void GivenFinalSayOutcome_WhenCanWriteFor_ThenTrue()
    {
        // Arrange
        var writer = new OutputOutcomeIoWriter(new TestCliIo(), new KitCliSettings());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new FinalSayOutcome("done")), Is.True);
    }

    [Test]
    public void GivenOtherOutcome_WhenCanWriteFor_ThenFalse()
    {
        // Arrange
        var writer = new OutputOutcomeIoWriter(new TestCliIo(), new KitCliSettings());

        // Act & Assert
        Assert.That(writer.CanWriteFor(new NothingOutcome()), Is.False);
    }

    [Test]
    public void GivenFinalSayOutcome_WhenWrite_ThenSaysItsMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new OutputOutcomeIoWriter(cliIo, new KitCliSettings());

        // Act
        writer.Write(new FinalSayOutcome("done"));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "done" }).AsCollection);
    }

    [Test]
    public void GivenLeveledFinalSayOutcomeAtMinimumLevel_WhenWrite_ThenSaysItsMessage()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new OutputOutcomeIoWriter(cliIo, new KitCliSettings { MinimumMessageLevel = MessageLevel.Warning });

        // Act
        writer.Write(new LeveledFinalSayOutcome("careful", MessageLevel.Warning));

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[] { "careful" }).AsCollection);
    }

    [Test]
    public void GivenLeveledFinalSayOutcomeBelowMinimumLevel_WhenWrite_ThenSaysNothing()
    {
        // Arrange
        var cliIo = new TestCliIo();
        var writer = new OutputOutcomeIoWriter(cliIo, new KitCliSettings { MinimumMessageLevel = MessageLevel.Warning });

        // Act
        writer.Write(new LeveledFinalSayOutcome("quiet", MessageLevel.Debug));

        // Assert
        Assert.That(cliIo.Lines, Is.Empty);
    }
}
