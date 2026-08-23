namespace KitCli.Abstractions.Tables;

/// <summary>
/// Specifies the direction in which a table column should be sorted.
/// </summary>
public enum CliTableSortOrder
{
    /// <summary>
    /// Sort from lowest to highest value.
    /// </summary>
    Ascending,

    /// <summary>
    /// Sort from highest to lowest value.
    /// </summary>
    Descending
}