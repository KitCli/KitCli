using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class PageSizeArtefactFactoryTests
{
    [Test]
    public void GivenPageSizeOutcome_WhenArtefactIsCreated_ThenItCarriesThePageSizeUnderItsName()
    {
        // Arrange
        var factory = new PageSizeArtefactFactory();

        // Act
        var artefact = (PageSizeArtefact)factory.Create(new PageSizeOutcome(50));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.EqualTo(50));
            Assert.That(artefact.Name, Is.EqualTo(nameof(PageSizeArtefact.PageSize)));
        });
    }
}
