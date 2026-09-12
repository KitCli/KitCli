namespace KitCli.Abstractions.Tables;

/// <summary>
/// Specifies how a table's lines are drawn: the frame around it, the dividers between its columns,
/// the rule under its headers, and whether a line separates each row.
/// </summary>
/// <remarks>
/// Only the styles drawing a box draw a line between every row. The rest leave the rows to run on,
/// because lines between the rows of a table with no frame around it read as clutter.
/// </remarks>
public enum CliTableStyle
{
    /// <summary>
    /// A box of <c>+</c>, <c>-</c> and <c>|</c> characters, with an unbroken line along the top and
    /// bottom. The default, and what every table has drawn until now.
    /// </summary>
    Ascii,

    /// <summary>
    /// The same box of <c>+</c>, <c>-</c> and <c>|</c> characters, with the columns marked along the
    /// top and bottom too.
    /// </summary>
    AsciiGrid,

    /// <summary>
    /// The same box of <c>+</c>, <c>-</c> and <c>|</c> characters, with a line of <c>=</c> under the
    /// column headers.
    /// </summary>
    AsciiDoubleHeader,

    /// <summary>
    /// A box of thin drawn lines with square corners.
    /// </summary>
    Box,

    /// <summary>
    /// A box of thin drawn lines with rounded corners.
    /// </summary>
    RoundedBox,

    /// <summary>
    /// A box of thick drawn lines throughout.
    /// </summary>
    ThickBox,

    /// <summary>
    /// A box whose outside is thick and whose inside lines are thin.
    /// </summary>
    ThickEdgedBox,

    /// <summary>
    /// A box whose column headers sit in a thick frame of their own, above a thin one holding the
    /// rows.
    /// </summary>
    ThickHeaderBox,

    /// <summary>
    /// A box of doubled lines throughout.
    /// </summary>
    DoubleBox,

    /// <summary>
    /// A box whose outside is doubled and whose inside lines are thin.
    /// </summary>
    DoubleEdgedBox,

    /// <summary>
    /// No box: a thin line between the columns, and a thin line under the column headers.
    /// </summary>
    Dividers,

    /// <summary>
    /// No box: a thin line between the columns, and a thick line under the column headers.
    /// </summary>
    ThickHeaderDividers,

    /// <summary>
    /// No box: a thin line between the columns, and a doubled line under the column headers.
    /// </summary>
    DoubleHeaderDividers,

    /// <summary>
    /// Nothing but a thin line under the column headers, running the width of the table.
    /// </summary>
    HeaderLine,

    /// <summary>
    /// Nothing but a thick line under the column headers, running the width of the table.
    /// </summary>
    ThickHeaderLine,

    /// <summary>
    /// A thin line under the column headers, with the surrounding blank space trimmed away so the
    /// table sits flush against the left margin.
    /// </summary>
    CompactHeaderLine,

    /// <summary>
    /// Thin lines under the column headers and along the top and bottom, with nothing between the
    /// columns.
    /// </summary>
    HeaderAndEdgeLines,

    /// <summary>
    /// A table written the way Markdown wants one, so the output can be pasted into an issue, a pull
    /// request or a document and render there as a table.
    /// </summary>
    Markdown,

    /// <summary>
    /// No rules at all, leaving the columns lined up by spacing alone.
    /// </summary>
    None
}
