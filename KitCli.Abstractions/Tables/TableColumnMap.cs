namespace KitCli.Abstractions.Tables;

/// <summary>
/// Describes how a single member of an aggregate type is rendered as a table column.
/// </summary>
/// <param name="memberName">The name of the member this map describes, used as the default column name.</param>
public class TableColumnMap(string memberName)
{
    /// <summary>
    /// The name displayed for this column. Defaults to the mapped member's name.
    /// </summary>
    public string ColumnName = memberName;

    /// <summary>
    /// Overrides the displayed column name.
    /// </summary>
    /// <param name="customName">The column name to display instead of the member's name.</param>
    /// <returns>The same <see cref="TableColumnMap"/> instance, to allow chaining.</returns>
    public TableColumnMap Name(string customName)
    {
        ColumnName = customName;
        return this;
    }
}