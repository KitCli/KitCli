using KitCli.Commands.Abstractions.Extensions;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Extensions;

[TestFixture]
public class CommandStringExtensionsTests
{
    [Test]
    [TestCase("SampleCliCommand", "Sample")]
    [TestCase("SampleTwoCliCommand", "SampleTwo")]
    [TestCase("SampleCommand", "SampleCommand")]
    public void GivenFullCommandName_WhenStripInstructionName_InstructionNameIsReturned(string commandName, string expectedStrippedName)
    {
        // Act
        var strippedName = commandName.ReplaceCommandSuffix();

        // Assert
        Assert.That(strippedName, Is.EqualTo(expectedStrippedName));
    }
}