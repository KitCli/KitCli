using KitCli.Commands.Abstractions.Artefacts.TableBuilder;
using KitCli.Commands.Abstractions.Outcomes.Anonymous;
using KitCli.Commands.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Commands.Abstractions.Tests.Artefacts;

[TestFixture]
public class TableBuilderArtefactFactoryTests
{
    [Test]
    public void GivenTableBuilderOutcome_WhenArtefactIsCreated_ThenItCarriesTheBuilderUnderItsTypeName()
    {
        // Arrange
        var factory = new TableBuilderArtefactFactory<TestAggregate, TestAggregate>();
        var tableBuilder = new TestTableBuilder();

        // Act
        var artefact = (TableBuilderArtefact<TestAggregate, TestAggregate>)factory.Create(
            new TableBuilderOutcome<TestAggregate, TestAggregate>(tableBuilder));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(artefact.Value, Is.SameAs(tableBuilder));
            Assert.That(artefact.Name, Is.EqualTo(nameof(TestTableBuilder)));
        });
    }
}
