using KitCli.Abstractions.Tables;
using NUnit.Framework;

namespace KitCli.Abstractions.Tests.Tables;

[TestFixture]
public class TableStyleTests
{
    [Test]
    public void GivenNewTable_WhenConstructed_ThenStyleIsAscii()
    {
        // Arrange & Act
        var table = new Table();

        // Assert
        Assert.That(table.Style, Is.EqualTo(CliTableStyle.Ascii));
    }

    [Test]
    public void GivenAsciiStyle_WhenToString_ThenTheBoxAndTheLinesBetweenRowsAreDrawn()
    {
        // Arrange
        var table = TwoRowTable(CliTableStyle.Ascii);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(rendered, Does.Contain("+---"));
            Assert.That(DrawnLinesIn(rendered), Is.EqualTo(4));
        });
    }

    [Test]
    public void GivenHeaderLineStyle_WhenToString_ThenOnlyTheLineUnderTheHeadersIsDrawn()
    {
        // Arrange
        var table = TwoRowTable(CliTableStyle.HeaderLine);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(rendered, Does.Not.Contain("|"));
            Assert.That(DrawnLinesIn(rendered), Is.EqualTo(1));
        });
    }

    [Test]
    public void GivenTheNoneStyle_WhenToString_ThenNothingButTheHeadersAndRowsIsDrawn()
    {
        // Arrange
        var table = TwoRowTable(CliTableStyle.None);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(DrawnLinesIn(rendered), Is.Zero);
            Assert.That(CellTextLinesIn(rendered), Is.EqualTo(3));
        });
    }

    [Test]
    public void GivenMarkdownStyle_WhenToString_ThenTheHeadersSitAboveAMarkdownDividerRow()
    {
        // Arrange
        var table = TwoRowTable(CliTableStyle.Markdown);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.That(NonEmptyLinesIn(rendered), Has.One.Matches<string>(IsAMarkdownDividerRow));
    }

    [TestCaseSource(nameof(EveryStyle))]
    public void GivenAnyStyle_WhenToString_ThenEveryHeaderAndValueIsStillRendered(CliTableStyle style)
    {
        // Arrange
        var table = TwoRowTable(style);

        // Act
        var rendered = table.ToString();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(rendered, Does.Contain("Command"));
            Assert.That(rendered, Does.Contain("Description"));
            Assert.That(rendered, Does.Contain("/exit"));
            Assert.That(rendered, Does.Contain("End the session."));
        });
    }

    [TestCaseSource(nameof(EveryStyle))]
    public void GivenAStyleWithNoBoxAroundIt_WhenToString_ThenNoLineIsDrawnBetweenTheRows(CliTableStyle style)
    {
        // Arrange & Act
        var drawnLinesGainedByOneMoreRow =
            DrawnLinesIn(TableOf(style, 3).ToString()) -
            DrawnLinesIn(TableOf(style, 2).ToString());

        // Assert
        Assert.That(drawnLinesGainedByOneMoreRow, Is.EqualTo(DrawsABox(style) ? 1 : 0));
    }

    private static bool IsAMarkdownDividerRow(string line)
        => line.StartsWith("| -") && line.EndsWith("|") && line.Contains(" | ");

    private static IEnumerable<CliTableStyle> EveryStyle() => Enum.GetValues<CliTableStyle>();

    private static bool DrawsABox(CliTableStyle style) => BoxStyles.Contains(style);

    private static readonly CliTableStyle[] BoxStyles =
    [
        CliTableStyle.Ascii,
        CliTableStyle.AsciiGrid,
        CliTableStyle.AsciiDoubleHeader,
        CliTableStyle.Box,
        CliTableStyle.RoundedBox,
        CliTableStyle.ThickBox,
        CliTableStyle.ThickEdgedBox,
        CliTableStyle.ThickHeaderBox,
        CliTableStyle.DoubleBox,
        CliTableStyle.DoubleEdgedBox
    ];

    private static Table TwoRowTable(CliTableStyle style) => TableOf(style, 2);

    private static Table TableOf(CliTableStyle style, int rowCount)
        => new(
            ["Command", "Description"],
            Enumerable
                .Range(1, rowCount)
                .Select(_ => new List<object> { "/exit", "End the session." })
                .ToList())
        {
            Style = style
        };

    private static int CellTextLinesIn(string rendered)
        => NonEmptyLinesIn(rendered).Count(line => line.Any(char.IsLetterOrDigit));

    private static int DrawnLinesIn(string rendered)
        => NonEmptyLinesIn(rendered).Count(line => !line.Any(char.IsLetterOrDigit));

    private static IEnumerable<string> NonEmptyLinesIn(string rendered)
        => rendered
            .Split(Environment.NewLine)
            .Where(line => line.Trim().Length > 0);
}
