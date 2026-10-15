using KitCli.Commands.Abstractions.Io;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Io;

[TestFixture]
public class CliIoExtensionsTests
{
    [Test]
    public void GivenNameAndDescription_WhenSayCommandSuggestion_ThenPausesThenSaysIndentedNameAndDescription()
    {
        // Arrange
        var cliIo = new TestCliIo();

        // Act
        cliIo.SayCommandSuggestion("/list", "Lists things");

        // Assert
        Assert.That(cliIo.Lines, Is.EqualTo(new[]
        {
            string.Empty,
            $"{SuggestionOutcomeIoWriter.NameSpacing}/list",
            $"{SuggestionOutcomeIoWriter.DescriptionSpacing}Lists things",
        }).AsCollection);
    }
}
