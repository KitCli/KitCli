namespace KitCli.Abstractions.Tables;

public class TableColumnMap(string memberName)
{
    public string ColumnName = memberName;
    
    public TableColumnMap Name(string customName)
    {
        ColumnName = customName;
        return this;
    }
}