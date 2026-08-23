using ConsoleTables;

namespace KitCli.Abstractions.Tables;

/// <summary>
/// A simple in-memory representation of tabular data, with support for rendering it as a formatted
/// console table.
/// </summary>
public class Table
{
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
        var table = new ConsoleTable
        {
            Options =
            {
                // I do it in the output formatting
                EnableCount = false
            }
        };

        table.AddColumn(Columns.ToArray());
       
        foreach (var row in Rows)
            table.AddRow(row.ToArray());
        
        return table.ToString();
    }
}