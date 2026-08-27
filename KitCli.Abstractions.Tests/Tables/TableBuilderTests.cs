using KitCli.Abstractions.Tables;
using KitCli.Abstractions.Tests.TestHelpers;
using NUnit.Framework;

namespace KitCli.Abstractions.Tests.Tables;

[TestFixture]
public class TableBuilderTests
{
    [Test]
    public void GivenBuilder_WhenConfigured_ThenEveryWithMethodReturnsTheSameBuilder()
    {
        // Arrange
        var builder = new TestTableBuilder();

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.That(builder.WithAggregator(new TestAggregator()), Is.SameAs(builder));
            Assert.That(builder.WithMap<TestTableMap>(), Is.SameAs(builder));
            Assert.That(builder.WithPageSize(10), Is.SameAs(builder));
            Assert.That(builder.WithPageNumber(1), Is.SameAs(builder));
            Assert.That(builder.WithMaxColumnWidth(40), Is.SameAs(builder));
        });
    }

    [Test]
    public void GivenNoAggregator_WhenBuild_ThenThrows()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithMap<TestTableMap>()
            .WithPageSize(10)
            .WithPageNumber(1);

        // Act & Assert
        Assert.That(() => builder.Build(), Throws.Exception.With.Message.EqualTo("Aggregator not initialized"));
    }

    [Test]
    public void GivenNoMap_WhenBuild_ThenThrows()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithAggregator(new TestAggregator())
            .WithPageSize(10)
            .WithPageNumber(1);

        // Act & Assert
        Assert.That(() => builder.Build(), Throws.Exception.With.Message.EqualTo("Map not initialized"));
    }

    [Test]
    public void GivenNoPageSize_WhenBuild_ThenThrows()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithAggregator(new TestAggregator())
            .WithMap<TestTableMap>()
            .WithPageNumber(1);

        // Act & Assert
        Assert.That(() => builder.Build(), Throws.Exception.With.Message.EqualTo("Page size not initialized"));
    }

    [Test]
    public void GivenNoPageNumber_WhenBuild_ThenThrows()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithAggregator(new TestAggregator())
            .WithMap<TestTableMap>()
            .WithPageSize(10);

        // Act & Assert
        Assert.That(() => builder.Build(), Throws.Exception.With.Message.EqualTo("Page number not initialized"));
    }

    [Test]
    public void GivenAMapWithARenamedColumn_WhenBuild_ThenColumnsFollowTheMap()
    {
        // Arrange
        var builder = ConfiguredBuilder(new TestAggregate("alpha", "the first value"));

        // Act
        var table = builder.Build();

        // Assert
        Assert.That(table.Columns, Is.EqualTo(new List<string> { "Label", "Description" }));
    }

    [Test]
    public void GivenAnUnmappedMember_WhenBuild_ThenThrows()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithAggregator(new TestAggregator(new TestAggregate("alpha", "the first value")))
            .WithMap<TestPartialTableMap>()
            .WithPageSize(10)
            .WithPageNumber(1);

        // Act & Assert
        Assert.That(() => builder.Build(), Throws.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public void GivenAggregates_WhenBuild_ThenEachBecomesARowOfMemberValues()
    {
        // Arrange
        var builder = ConfiguredBuilder(
            new TestAggregate("alpha", "the first value"),
            new TestAggregate("beta", "the second value"));

        // Act
        var table = builder.Build();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(table.Rows, Has.Count.EqualTo(2));
            Assert.That(table.Rows[0], Is.EqualTo(new List<object> { "alpha", "the first value" }));
            Assert.That(table.Rows[1], Is.EqualTo(new List<object> { "beta", "the second value" }));
        });
    }

    [Test]
    public void GivenAPageSizeAndNumber_WhenBuild_ThenOnlyThatPageIsRendered()
    {
        // Arrange
        var builder = new TestTableBuilder()
            .WithAggregator(new TestAggregator(
                new TestAggregate("alpha", "the first value"),
                new TestAggregate("beta", "the second value")))
            .WithMap<TestTableMap>()
            .WithPageSize(1)
            .WithPageNumber(2);

        // Act
        var table = builder.Build();

        // Assert
        Assert.That(table.Rows[0], Is.EqualTo(new List<object> { "beta", "the second value" }));
    }

    [Test]
    public void GivenNoMaxColumnWidth_WhenBuild_ThenTheTableBreaksNoCell()
    {
        // Arrange
        var builder = ConfiguredBuilder(new TestAggregate("alpha", "the first value"));

        // Act
        var table = builder.Build();

        // Assert
        Assert.That(table.MaxColumnWidth, Is.EqualTo(Table.DefaultMaxColumnWidth));
    }

    [Test]
    public void GivenAMaxColumnWidth_WhenBuild_ThenTheTableCarriesIt()
    {
        // Arrange
        var builder = ConfiguredBuilder(new TestAggregate("alpha", "the first value"))
            .WithMaxColumnWidth(40);

        // Act
        var table = builder.Build();

        // Assert
        Assert.That(table.MaxColumnWidth, Is.EqualTo(40));
    }

    private static TableBuilder<TestAggregate, TestAggregate> ConfiguredBuilder(params TestAggregate[] aggregates)
        => new TestTableBuilder()
            .WithAggregator(new TestAggregator(aggregates))
            .WithMap<TestTableMap>()
            .WithPageSize(10)
            .WithPageNumber(1);
}
