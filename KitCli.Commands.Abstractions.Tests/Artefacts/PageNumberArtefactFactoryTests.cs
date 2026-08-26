using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class PageNumberArtefactFactoryTests
{
    [Test]
    public void GivenPageNumberOutcome_WhenArtefactIsCreated_ThenItCarriesThePageNumberUnderItsName()
    {
        // Arrange
        var factory = new PageNumberArtefactFactory();

        // Act
        var artefact = (PageNumberArtefact)factory.Create(new PageNumberOutcome(2));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.EqualTo(2));
            Assert.That(artefact.Name, Is.EqualTo(nameof(PageNumberArtefact.PageNumber)));
        });
    }
}
