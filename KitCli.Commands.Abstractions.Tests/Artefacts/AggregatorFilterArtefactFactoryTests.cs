using KitCli.Abstractions.Aggregators;
using KitCli.Commands.Abstractions.Artefacts.Aggregator.Filters;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class AggregatorFilterArtefactFactoryTests
{
    [Test]
    public void GivenAggregatorFilterOutcome_WhenArtefactIsCreated_ThenItCarriesTheFilterUnderItsFullName()
    {
        // Arrange
        var factory = new AggregatorFilterArtefactFactory();
        var filter = new AggregatorFilter("Category", "Equals", "Groceries");

        // Act
        var artefact = (AggregatorFilterArtefact)factory.Create(new AggregatorFilterOutcome(filter));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.SameAs(filter));
            Assert.That(artefact.Name, Is.EqualTo(filter.FullName));
        });
    }
}
