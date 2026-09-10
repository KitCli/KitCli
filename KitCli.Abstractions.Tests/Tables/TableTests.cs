using KitCli.Abstractions.Tables;
using NUnit.Framework;

namespace KitCli.Abstractions.Tests.Tables;

[TestFixture]
public class TableTests
{
    private const string LongValue = "a value long enough that the library would break it across two lines";

    [Test]
    public void GivenNoColumnsOrRows_WhenConstructed_ThenTableIsEmpty()
    {
        // Arrange & Act
        var table = new Table();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(table.Columns, Is.Empty);
            Assert.That(table.Rows, Is.Empty);
        });
    }

    [Test]
    public void GivenColumnsAndRows_WhenConstructed_ThenTableHoldsThem()
    {
        // Arrange & Act
        var table = new Table(["Name", "Description"], [["alpha", LongValue]]);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(table.Columns, Is.EqualTo(new List<string> { "Name", "Description" }));
            Assert.That(table.Rows, Has.Count.EqualTo(1));
            Assert.That(table.Rows[0], Is.EqualTo(new List<object> { "alpha", LongValue }));
        });
    }

    [Test]
    public void GivenNewTable_WhenConstructed_ThenMaxColumnWidthIsTheDefault()
    {
        // Arrange & Act
        var table = new Table();

        // Assert
        Assert.That(table.MaxColumnWidth, Is.EqualTo(Table.DefaultMaxColumnWidth));
    }

    [Test]
    public void GivenColumnsAndRows_WhenToString_ThenEveryHeaderAndValueIsRendered()
    {
        // Arrange
        var table = new Table(["Name", "Description"], [["alpha", LongValue]]);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(rendered, Does.Contain("Name"));
            Assert.That(rendered, Does.Contain("Description"));
            Assert.That(rendered, Does.Contain("alpha"));
            Assert.That(rendered, Does.Contain(LongValue));
        });
    }

    [Test]
    public void GivenCellLongerThanFortyCharacters_WhenToString_ThenOneRowIsRenderedPerRowOfData()
    {
        // Arrange
        var table = new Table(["Name", "Description"], [["alpha", LongValue]]);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.That(CellLinesIn(rendered), Is.EqualTo(2));
    }

    [Test]
    public void GivenMaxColumnWidth_WhenToString_ThenLongerCellIsBrokenAcrossLines()
    {
        // Arrange
        var table = new Table(["Name", "Description"], [["alpha", LongValue]])
        {
            MaxColumnWidth = 40
        };

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.That(CellLinesIn(rendered), Is.EqualTo(3));
    }

    [Test]
    public void GivenAnyTable_WhenToString_ThenNoRowCountIsAppended()
    {
        // Arrange
        var table = new Table(["Name"], [["alpha"]]);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.That(rendered, Does.Not.Contain("Count:"));
    }

    [Test]
    public void GivenCellContainingNewLines_WhenToString_ThenItStaysInsideOneRow()
    {
        // Arrange
        var table = new Table(["Name", "Description"], [["alpha", "first line\nsecond line"]]);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(CellLinesIn(rendered), Is.EqualTo(3));
            Assert.That(EveryLineIsTheSameWidth(rendered), Is.True);
        });
    }

    [Test]
    public void GivenCellContainingSquareBrackets_WhenToString_ThenTheyAreRenderedLiterally()
    {
        // Arrange
        var bracketed = "List`1[System.String]";
        var table = new Table(["Type"], [[bracketed]]);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.That(rendered, Does.Contain(bracketed));
    }

    private static bool EveryLineIsTheSameWidth(string rendered)
        => rendered
            .Split(Environment.NewLine)
            .Where(line => line.Length > 0)
            .Select(line => line.Length)
            .Distinct()
            .Count() == 1;

    private static int CellLinesIn(string rendered)
        => rendered
            .Split(Environment.NewLine)
            .Count(HoldsCellText);

    private static bool HoldsCellText(string line)
        => line.Contains('|') && line.Any(character => !BorderCharacters.Contains(character));

    private static readonly char[] BorderCharacters = ['|', '-', '+'];
}
