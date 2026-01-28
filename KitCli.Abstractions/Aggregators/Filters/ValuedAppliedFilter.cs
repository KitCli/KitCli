namespace KitCli.Abstractions.Aggregators.Filters;

public class ValuedCliListAggregatorFilter<TFilterValue> : CliListAggregatorFilter
    where TFilterValue : notnull
{
    public TFilterValue FilterValue { get; }
    
    public ValuedCliListAggregatorFilter(
        string filterFieldName,
        string filterName,
        TFilterValue filterValue)
        : base(filterFieldName, filterName)
    {
        FilterValue = filterValue;
    }
}