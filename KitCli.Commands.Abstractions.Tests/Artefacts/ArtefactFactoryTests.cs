using KitCli.Commands.Abstractions.Artefacts.Page;
using KitCli.Commands.Abstractions.Outcomes.Final;
using KitCli.Commands.Abstractions.Outcomes.Reusable.Page;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

/// <summary>
/// Covers the outcome-type matching <see cref="Abstractions.Artefacts.ArtefactFactory{TOutcome}"/> handles for
/// every implementation, through one of them.
/// </summary>
[TestFixture]
public class ArtefactFactoryTests
{
    private PageNumberArtefactFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new PageNumberArtefactFactory();
    }

    [Test]
    public void GivenItsOwnOutcome_WhenForIsCalled_ThenItMatches()
    {
        // Act
        var matches = _factory.For(new PageNumberOutcome(2));

        // Assert
        Assert.That(matches, Is.True);
    }

    [Test]
    public void GivenAnotherOutcome_WhenForIsCalled_ThenItDoesNotMatch()
    {
        // Act
        var matches = _factory.For(new NothingOutcome());

        // Assert
        Assert.That(matches, Is.False);
    }

    [Test]
    public void GivenItsOwnOutcome_WhenArtefactIsCreated_ThenTheImplementationsArtefactIsReturned()
    {
        // Act
        var artefact = _factory.Create(new PageNumberOutcome(2));

        // Assert
        Assert.That(artefact, Is.InstanceOf<PageNumberArtefact>());
    }

    [Test]
    public void GivenAnotherOutcome_WhenArtefactIsCreated_ThenItThrows()
    {
        // Assert
        Assert.That(() => _factory.Create(new NothingOutcome()), Throws.InvalidOperationException);
    }
}
