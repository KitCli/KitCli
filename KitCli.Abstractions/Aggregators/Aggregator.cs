namespace KitCli.Abstractions.Aggregators;

public abstract record Aggregator<TSource, TAggregate>
{
    private readonly List<Func<IEnumerable<TSource>, IEnumerable<TSource>>> _sourceFunctions = [];
    private readonly List<Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>>> _aggregateFunctions = [];

    public IEnumerable<TSource> Source { get; init; }
    
    public Aggregator(IEnumerable<TSource> source)
    {
        Source = source;
    }
    
    /// <summary>
    /// Aggregate the result set.
    /// </summary>
    public IEnumerable<TAggregate> Aggregate(int pageSize, int pageNumber)
    {
        var clone = new List<TSource>(Source.ToList()).AsEnumerable();

        clone = _sourceFunctions.Aggregate(clone, (current, sourceFunction) => sourceFunction(current));

        var aggregates = DoAggregation(clone);

        aggregates = _aggregateFunctions.Aggregate(aggregates, (current, aggregateFunction) => aggregateFunction(current));

        var skipNumber = pageSize * (pageNumber - 1);
        
        aggregates = aggregates.Skip(skipNumber).ToList();

        return aggregates.Take(pageSize);
    }
    
    public Aggregator<TSource, TAggregate> BeforeAggregation(Func<IEnumerable<TSource>, IEnumerable<TSource>> operationFunction)
    {
        _sourceFunctions.Add(operationFunction);
        return this;
    }
    
    /// <summary>
    /// Perform an operation on the result set after aggregation. This can be used for operations such as sorting, filtering, etc.
    /// </summary>
    public Aggregator<TSource, TAggregate> AfterAggregation(Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction)
    {
        _aggregateFunctions.Add(operationFunction);
        return this;
    }

    protected abstract IEnumerable<TAggregate> DoAggregation(IEnumerable<TSource> source);
}