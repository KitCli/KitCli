using ConsoleTables;

namespace KitCli.Abstractions.Tables;

public class CliTable
{
    public List<string> Columns { get; set; } = [];
    public List<List<object>> Rows { get; set; } = [];

    public bool ShowRowCount { get; set; } = true;

    public override string ToString()
    {
        var table = new ConsoleTable
        {
            Options =
            {
                EnableCount = ShowRowCount
            }
        };

        table.AddColumn(Columns.ToArray());
       
        foreach (var row in Rows)
            table.AddRow(row.ToArray());
        
        return table.ToString();
    }
}