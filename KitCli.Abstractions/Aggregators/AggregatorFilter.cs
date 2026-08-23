namespace KitCli.Abstractions.Aggregators;

/// <summary>
/// Describes a single filter applied to a field, identified by the field it targets, the name of the filter,
/// and the value the filter is applied with.
/// </summary>
/// <param name="FilterFieldName">The name of the field the filter is applied to.</param>
/// <param name="FilterName">The name of the filter being applied.</param>
/// <param name="FilterValue">The value the filter is applied with.</param>
public record AggregatorFilter(string FilterFieldName, string FilterName, object FilterValue)
{
    /// <summary>
    /// A composite identifier combining the field name, filter name, and filter value, in the form
    /// <c>FilterFieldName.FilterName-FilterValue</c>.
    /// </summary>
    public string FullName => $"{FilterFieldName}.{FilterName}-{FilterValue}";
}