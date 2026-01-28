using KitCli.Commands.Abstractions.Artefacts;
using KitCli.Commands.Abstractions.Artefacts.CommandRan;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class WhenLastCommandRanWasTests
{
    private record TestCliCommand : CliCommand;
    
    [Test]
    public void GivenRanCommandArtefact_WhenLastCommandRanWas_ThenShouldReturnTrue()
    {
        // Arrange
        var artefacts = new List<CliCommandArtefact>
        {
            new RanCliCommandArtefact(new TestCliCommand())
        };
        
        // Act
        var result = artefacts.LastCommandRanWas<TestCliCommand>();
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void GivenNoRanCommandArtefact_WhenLastCommandRanWas_ThenShouldReturnFalse()
    {
        // Arrange
        var artefacts = new List<CliCommandArtefact>();
        
        // Act
        var result = artefacts.LastCommandRanWas<TestCliCommand>();
        
        // Assert
        Assert.That(result, Is.False);
    }
}