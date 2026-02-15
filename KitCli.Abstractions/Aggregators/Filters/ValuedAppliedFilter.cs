namespace KitCli.Abstractions.Aggregators.Filters;

public record AggregatorFilter<TFilterValue>(string FilterFieldName, string FilterName, TFilterValue FilterValue) 
    : AnonymousAggregatorFilter(FilterFieldName, FilterName)
    where TFilterValue : notnull;