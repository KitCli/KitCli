using Spectre.Console;
using Spectre.Console.Rendering;

namespace KitCli.Abstractions.Tables;

/// <summary>
/// A simple in-memory representation of tabular data, with support for rendering it as a formatted
/// console table.
/// </summary>
public class Table
{
    /// <summary>
    /// The <see cref="MaxColumnWidth"/> at which no column is given a width of its own, leaving the
    /// renderer to size every column to fit the console.
    /// </summary>
    public const int DefaultMaxColumnWidth = int.MaxValue;

    /// <summary>
    /// The width a column is held to, breaking its text across lines to fit. Applied only to a
    /// column already wider than this. Defaults to <see cref="DefaultMaxColumnWidth"/>.
    /// </summary>
    public int MaxColumnWidth { get; set; } = DefaultMaxColumnWidth;

    /// <summary>
    /// The column headers, in display order.
    /// </summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>
    /// The table rows; each row is a list of cell values corresponding to <see cref="Columns"/>.
    /// </summary>
    public List<List<object>> Rows { get; set; } = [];

    /// <summary>
    /// Initializes a new, empty instance of the <see cref="Table"/> class.
    /// </summary>
    public Table()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Table"/> class with the given columns and rows.
    /// </summary>
    /// <param name="columns">The column headers, in display order.</param>
    /// <param name="rows">The table rows; each row is a list of cell values corresponding to <paramref name="columns"/>.</param>
    public Table(List<string> columns, List<List<object>> rows)
    {
        Columns = columns;
        Rows = rows;
    }

    /// <summary>
    /// Renders the table as a formatted, fixed-width string suitable for console output.
    /// </summary>
    /// <returns>The formatted table.</returns>
    public override string ToString()
    {
        var table = new Spectre.Console.Table
        {
            Border = TableBorder.Ascii,
            ShowRowSeparators = true
        };

        for (var index = 0; index < Columns.Count; index++)
            table.AddColumn(ColumnAt(index));

        foreach (var row in Rows)
            table.AddRow(row.Select(AsPlainText).ToArray<IRenderable>());

        return Render(table);
    }

    private TableColumn ColumnAt(int index)
    {
        var column = new TableColumn(AsPlainText(Columns[index]));

        if (MaxColumnWidth != DefaultMaxColumnWidth && WidestLineIn(index) > MaxColumnWidth)
            column.Width = MaxColumnWidth;

        return column;
    }

    private int WidestLineIn(int index)
        => Rows
            .Select(row => index < row.Count ? row[index] : null)
            .Append(Columns[index])
            .SelectMany(value => (value?.ToString() ?? string.Empty).Split('\n'))
            .Max(line => line.TrimEnd('\r').Length);

    /// <summary>
    /// Wraps a value so the renderer prints it literally, rather than reading square brackets in it
    /// as markup.
    /// </summary>
    private static Text AsPlainText(object? value)
        => new(value?.ToString() ?? string.Empty);

    /// <summary>
    /// Renders to a string that fits the console it is about to be printed to, falling back to
    /// eighty columns when there is no console to measure.
    /// </summary>
    private static string Render(Spectre.Console.Table table)
    {
        var writer = new StringWriter();

        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No,
            Out = new AnsiConsoleOutput(writer)
        });

        console.Write(table);

        return writer.ToString();
    }
}
