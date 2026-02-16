using System.Reflection;
using KitCli.Abstractions.Aggregators;

namespace KitCli.Abstractions.Tables;

public abstract class TableBuilder<TSource, TAggregate>
{
    private Aggregator<TSource, TAggregate>? _aggregator;
    private TableMap<TAggregate>? _map;
    private int? _pageSize = null;
    private int? _pageNumber = null;
    
    public TableBuilder<TSource, TAggregate> WithAggregator(Aggregator<TSource, TAggregate> aggregator)
    {
        _aggregator = aggregator;
        return this;
    }

    public TableBuilder<TSource, TAggregate> WithMap<TMapType>() where TMapType : TableMap<TAggregate>, new()
    {
        _map = new TMapType();
        return this;
    }
    
    public TableBuilder<TSource, TAggregate> WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }
    
    public TableBuilder<TSource, TAggregate> WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    // TODO: This needs cleaning up!
    public Table Build()
    {
        if (_aggregator == null)
        {
            throw new Exception("Aggregator not initialized");
        }
        
        // TODO: Feels like this should not be mandatory.
        // maybe default is possible?
        if (_map == null)
        {
            throw new Exception("Map not initialized");
        }
        
        if (_pageSize == null) 
        {
            throw new Exception("Page size not initialized");
        }
        
        if (_pageNumber == null) 
        {
            throw new Exception("Page number not initialized");
        }
        
        var aggregates = _aggregator.Aggregate(_pageSize.Value, _pageNumber.Value);

        var members = typeof(TAggregate).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var headerNames = new List<string>();
        foreach (var member in members)
        {
            var config = _map.ColumnMaps[member];
            headerNames.Add(config.ColumnName);
        }
        
        var rows = new List<List<object>>();
        foreach (var aggregate in aggregates)
        {
            var row = new List<object>();
            foreach (var member in members)
            {
                var config = _map.ColumnMaps[member];
                var value = member.GetValue(aggregate);
                
                // How does the config define the mapping?
                var mappedValue = value?.ToString() ?? string.Empty;
                
                row.Add(mappedValue);
            }

            rows.Add(row);
        }

        return new Table(headerNames, rows);
    }
}