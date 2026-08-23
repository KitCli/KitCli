namespace KitCli.Abstractions.Aggregators;

/// <summary>
/// Base type for turning a source sequence into a paged sequence of aggregate results, with optional
/// pre- and post-aggregation transforms (e.g. filtering, sorting) applied around the aggregation step.
/// </summary>
/// <typeparam name="TSource">The type of the elements being aggregated.</typeparam>
/// <typeparam name="TAggregate">The type of the aggregated results produced.</typeparam>
public abstract record Aggregator<TSource, TAggregate>
{
    private readonly List<Func<IEnumerable<TSource>, IEnumerable<TSource>>> _sourceFunctions = [];
    private readonly List<Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>>> _aggregateFunctions = [];

    /// <summary>
    /// The source elements to be aggregated.
    /// </summary>
    public IEnumerable<TSource> Source { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Aggregator{TSource, TAggregate}"/> class.
    /// </summary>
    /// <param name="source">The source elements to be aggregated.</param>
    public Aggregator(IEnumerable<TSource> source)
    {
        Source = source;
    }

    /// <summary>
    /// Applies any registered pre-aggregation transforms to <see cref="Source"/>, performs the aggregation,
    /// applies any registered post-aggregation transforms, then returns a single page of the resulting sequence.
    /// </summary>
    /// <param name="pageSize">The maximum number of aggregate results to return.</param>
    /// <param name="pageNumber">The 1-based page number to return.</param>
    /// <returns>The requested page of aggregate results.</returns>
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
    
    /// <summary>
    /// Registers an operation to run on the source set before aggregation. This can be used for operations
    /// such as filtering the source elements.
    /// </summary>
    /// <param name="operationFunction">The transform to apply to the source sequence before aggregation.</param>
    /// <returns>The same <see cref="Aggregator{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public Aggregator<TSource, TAggregate> BeforeAggregation(Func<IEnumerable<TSource>, IEnumerable<TSource>> operationFunction)
    {
        _sourceFunctions.Add(operationFunction);
        return this;
    }

    /// <summary>
    /// Perform an operation on the result set after aggregation. This can be used for operations such as sorting, filtering, etc.
    /// </summary>
    /// <param name="operationFunction">The transform to apply to the aggregated sequence after aggregation.</param>
    /// <returns>The same <see cref="Aggregator{TSource, TAggregate}"/> instance, to allow chaining.</returns>
    public Aggregator<TSource, TAggregate> AfterAggregation(Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction)
    {
        _aggregateFunctions.Add(operationFunction);
        return this;
    }

    /// <summary>
    /// Performs the actual aggregation of the (already source-transformed) elements into aggregate results.
    /// </summary>
    /// <param name="source">The source elements, after any <see cref="BeforeAggregation"/> transforms have been applied.</param>
    /// <returns>The aggregated results, before any <see cref="AfterAggregation"/> transforms are applied.</returns>
    protected abstract IEnumerable<TAggregate> DoAggregation(IEnumerable<TSource> source);
}