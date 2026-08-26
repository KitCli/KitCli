using KitCli.Commands.Abstractions.Artefacts.RanCliCommand;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class RanCliCommandArtefactFactoryTests
{
    [Test]
    public void GivenRanCliCommandOutcome_WhenArtefactIsCreated_ThenItCarriesTheCommandUnderItsTypeName()
    {
        // Arrange
        var factory = new RanCliCommandArtefactFactory();
        var command = new TestNextCliCommand();

        // Act
        var artefact = (RanCliCommandArtefact)factory.Create(new RanCliCommandOutcome(command));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.SameAs(command));
            Assert.That(artefact.Name, Is.EqualTo(nameof(TestNextCliCommand)));
        });
    }
}
