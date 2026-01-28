using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests;

[TestFixture]
public class CliCommandTests
{
    private record TestCliCommand : CliCommand;
    private record AnotherCommand : CliCommand;
    
    [Test]
    public void GivenTestCliCommand_WhenGetCommandNameIsCalled_ThenCorrectNameIsReturned()
    {
        // Arrange
        var command = new TestCliCommand();
        
        // Act
        var commandName = command.GetSpecificCommandName();

        // Assert
        var expectedCommandName = nameof(TestCliCommand).Replace(nameof(CliCommand), string.Empty);
        Assert.That(commandName, Is.EqualTo(expectedCommandName));
    }
    
    [Test]
    public void GivenAnotherCliCommand_WhenGetCommandNameIsCalled_ThenCorrectNameIsReturned()
    {
        // Arrange
        var command = new AnotherCommand();
        
        // Act
        var commandName = command.GetSpecificCommandName();

        // Assert
        Assert.That(commandName, Is.EqualTo("AnotherCommand"));
    }
    
    [Test]
    [TestCase("SampleCliCommand", "Sample")]
    [TestCase("SampleTwoCliCommand", "SampleTwo")]
    public void GivenCommandClassName_WhenStripCommandName_InstructionNameIsReturned(string commandClassName, string expectedStrippedName)
    {
        // Act
        var strippedName = CliCommand.StripCommandName(commandClassName);

        // Assert
        Assert.That(strippedName, Is.EqualTo(expectedStrippedName));
    }
}