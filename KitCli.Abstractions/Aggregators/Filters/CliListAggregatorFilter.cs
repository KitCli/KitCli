namespace KitCli.Abstractions.Aggregators.Filters;

public class CliListAggregatorFilter
{
    public string FilterFieldName { get; }
    public string FilterName { get; }
    
    public CliListAggregatorFilter(
        string filterFieldName,
        string filterName)
    {
        FilterFieldName = filterFieldName;
        FilterName = filterName;
    }
}