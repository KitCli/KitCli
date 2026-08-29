using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Factories;

[TestFixture]
public class CliCommandReactionFactoryTests
{
    private TestCliCommandReactionFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestCliCommandReactionFactory();
    }

    [Test]
    public void GivenUnattachedFactory_WhenArtefactsAreRead_ThenNoArtefactsAreReturned()
    {
        // Assert
        Assert.That(_factory.Artefacts, Is.Empty);
    }

    [Test]
    public void GivenUnattachedFactory_WhenArtefactsAreQueried_ThenItThrows()
    {
        // Assert
        Assert.That(() => _factory.GetArtefacts<int>().ToList(), Throws.Exception);
    }

    [Test]
    public void GivenAttachedFactory_WhenAttachIsCalled_ThenTheSameFactoryIsReturned()
    {
        // Act
        var attached = _factory.Attach([]);

        // Assert
        Assert.That(attached, Is.SameAs(_factory));
    }

    [Test]
    [TestCase("PageNumber", true)]
    [TestCase("Missing", false)]
    [TestCase(null, true)]
    public void GivenTypedArtefact_WhenAnyArtefactIsCalled_ThenTheNameFilterIsHonoured(
        string? artefactName,
        bool expectedMatch)
    {
        // Arrange
        _factory.Attach([new PageNumberArtefact(2)]);

        // Act
        var matches = _factory.AnyArtefact<int>(artefactName);

        // Assert
        Assert.That(matches, Is.EqualTo(expectedMatch));
    }

    [Test]
    public void GivenNoArtefactOfThatType_WhenAnyArtefactIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        _factory.Attach([new PageNumberArtefact(2)]);

        // Act
        var matches = _factory.AnyArtefact<string>();

        // Assert
        Assert.That(matches, Is.False);
    }

    [Test]
    public void GivenArtefactsSharingAType_WhenArtefactIsReadByName_ThenTheNamedOneIsReturned()
    {
        // Arrange
        _factory.Attach([new PageNumberArtefact(2), new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetArtefact<int>(nameof(PageNumberArtefact.PageNumber));

        // Assert
        Assert.That(artefact?.Value, Is.EqualTo(2));
    }

    [Test]
    public void GivenArtefactsSharingAType_WhenArtefactIsReadUnnamed_ThenTheLastOneIsReturned()
    {
        // Arrange
        _factory.Attach([new PageNumberArtefact(2), new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetArtefact<int>();

        // Assert
        Assert.That(artefact?.Value, Is.EqualTo(50));
    }

    [Test]
    public void GivenNoMatchingArtefact_WhenArtefactIsRead_ThenNothingIsReturned()
    {
        // Arrange
        _factory.Attach([new PageNumberArtefact(2)]);

        // Act
        var artefact = _factory.GetArtefact<int>("Missing");

        // Assert
        Assert.That(artefact, Is.Null);
    }

    [Test]
    public void GivenMatchingArtefact_WhenRequiredArtefactIsRead_ThenItIsReturned()
    {
        // Arrange
        _factory.Attach([new PageSizeArtefact(50)]);

        // Act
        var artefact = _factory.GetRequiredArtefact<int>(nameof(PageSizeArtefact.PageSize));

        // Assert
        Assert.That(artefact.Value, Is.EqualTo(50));
    }

    [Test]
    public void GivenNoMatchingArtefact_WhenRequiredArtefactIsRead_ThenItThrows()
    {
        // Arrange
        _factory.Attach([]);

        // Assert
        Assert.That(() => _factory.GetRequiredArtefact<int>(), Throws.Exception);
    }
}
