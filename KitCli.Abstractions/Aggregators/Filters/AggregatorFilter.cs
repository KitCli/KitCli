namespace KitCli.Abstractions.Aggregators.Filters;

public record AggregatorFilter(string FilterFieldName, string FilterName, object FilterValue)
{
    public string FullName => $"{FilterFieldName}.{FilterName}-{FilterValue}";
}