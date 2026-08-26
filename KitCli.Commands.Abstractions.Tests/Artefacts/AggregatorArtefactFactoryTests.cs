using KitCli.Commands.Abstractions.Artefacts.Aggregator;
using KitCli.Commands.Abstractions.Outcomes.Reusable;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class AggregatorArtefactFactoryTests
{
    [Test]
    public void GivenAggregatorOutcome_WhenArtefactIsCreated_ThenItCarriesTheAggregatorUnderItsTypeName()
    {
        // Arrange
        var factory = new AggregatorArtefactFactory<TestAggregate, TestAggregate>();
        var aggregator = new TestAggregator();

        // Act
        var artefact = (AggregatorArtefact<TestAggregate, TestAggregate>)factory.Create(
            new AggregatorOutcome<TestAggregate, TestAggregate>(aggregator));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.SameAs(aggregator));
            Assert.That(artefact.Name, Is.EqualTo(nameof(TestAggregator)));
        });
    }

    [Test]
    public void GivenAggregatorOutcomeOfAnotherPair_WhenForIsCalled_ThenItDoesNotMatch()
    {
        // Arrange
        var factory = new AggregatorArtefactFactory<TestAggregate, string>();

        // Act
        var matches = factory.For(new AggregatorOutcome<TestAggregate, TestAggregate>(new TestAggregator()));

        // Assert
        Assert.That(matches, Is.False);
    }
}
