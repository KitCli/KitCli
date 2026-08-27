using System.Reflection;
using KitCli.Abstractions.Aggregators;

namespace KitCli.Abstractions.Tables;

/// <summary>
/// Builds a <see cref="Table"/> by aggregating a source sequence and mapping the resulting aggregate members
/// to table columns, using the fluent <c>With...</c> methods to configure the aggregator, column map, and paging.
/// </summary>
/// <typeparam name="TSource">The type of the source elements being aggregated.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated results rendered as table rows.</typeparam>
public abstract class TableBuilder<TSource, TAggregate>
{
    private Aggregator<TSource, TAggregate>? _aggregator;
    private TableMap<TAggregate>? _map;
    private int? _pageSize;
    private int? _pageNumber;
    private int? _maxColumnWidth;

    /// <summary>
    /// Sets the aggregator used to turn the source sequence into aggregate results.
    /// </summary>
    /// <param name="aggregator">The aggregator to use.</param>
    /// <returns>The same <see cref="TableBuilder{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public TableBuilder<TSource, TAggregate> WithAggregator(Aggregator<TSource, TAggregate> aggregator)
    {
        _aggregator = aggregator;
        return this;
    }

    /// <summary>
    /// Sets the column map used to determine which members of <typeparamref name="TAggregate"/> are rendered
    /// as columns, and under what names.
    /// </summary>
    /// <typeparam name="TMapType">The concrete <see cref="TableMap{TAggregate}"/> type to instantiate and use.</typeparam>
    /// <returns>The same <see cref="TableBuilder{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public TableBuilder<TSource, TAggregate> WithMap<TMapType>() where TMapType : TableMap<TAggregate>, new()
    {
        _map = new TMapType();
        return this;
    }

    /// <summary>
    /// Sets the maximum number of rows to include in the built table.
    /// </summary>
    /// <param name="pageSize">The page size.</param>
    /// <returns>The same <see cref="TableBuilder{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public TableBuilder<TSource, TAggregate> WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Sets the page of aggregate results to render as the table's rows.
    /// </summary>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <returns>The same <see cref="TableBuilder{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public TableBuilder<TSource, TAggregate> WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    /// <summary>
    /// Sets the width a column's text may reach before a cell is broken across lines. Left unset,
    /// the built table breaks no cell.
    /// </summary>
    /// <param name="maxColumnWidth">The maximum column width.</param>
    /// <returns>The same <see cref="TableBuilder{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public TableBuilder<TSource, TAggregate> WithMaxColumnWidth(int maxColumnWidth)
    {
        _maxColumnWidth = maxColumnWidth;
        return this;
    }

    // TODO: This needs cleaning up!
    /// <summary>
    /// Runs the configured aggregator over its source, then maps the resulting page of aggregates into a
    /// <see cref="Table"/> using the configured column map.
    /// </summary>
    /// <returns>The built table.</returns>
    /// <exception cref="Exception">Thrown when the aggregator, column map, page size, or page number has not been configured via the corresponding <c>With...</c> method.</exception>
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
                // TODO: Use the config to determine how to map the value, not just ToString()!
                var unused = _map.ColumnMaps[member];
                var value = member.GetValue(aggregate);
                
                // How does the config define the mapping?
                var mappedValue = value?.ToString() ?? string.Empty;
                
                row.Add(mappedValue);
            }

            rows.Add(row);
        }

        return new Table(headerNames, rows)
        {
            MaxColumnWidth = _maxColumnWidth ?? Table.DefaultMaxColumnWidth
        };
    }
}