using ConsoleTables;

namespace KitCli.Abstractions.Tables;

public class Table
{
    public List<string> Columns { get; set; } = [];
    public List<List<object>> Rows { get; set; } = [];

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