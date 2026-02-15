namespace KitCli.Abstractions.Aggregators;

// TODO: Simplify aggregator naming.
public abstract class CliListAggregator<TAggregate> : CliAggregator<IEnumerable<TAggregate>>
{
    // TODO: If the filter commands can be linked to Pagination, this being public isn't needed.
    public readonly int PageSize;
    public readonly int PageNumber;
    
    private readonly List<Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>>> _aggregateFunctions = [];

    protected CliListAggregator(int? pageSize = null, int? pageNumber = null)
    {
        PageSize = pageSize ?? 20;
        PageNumber = pageNumber ?? 1;
    }
    
    public override IEnumerable<TAggregate> Aggregate()
    {
        var aggregates = ListAggregate();
        
        aggregates = _aggregateFunctions.Aggregate(aggregates, ApplyAggregateFunction);

        var skipNumber = PageSize * (PageNumber - 1);
        
        aggregates = aggregates.Skip(skipNumber);

        return aggregates.Take(PageSize);
    }
    public CliListAggregator<TAggregate> AfterAggregation(Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction)
    {
        _aggregateFunctions.Add(operationFunction);
        return this;
    }

    protected abstract IEnumerable<TAggregate> ListAggregate();
    
    private IEnumerable<TAggregate> ApplyAggregateFunction(IEnumerable<TAggregate> current, Func<IEnumerable<TAggregate>, IEnumerable<TAggregate>> operationFunction) => operationFunction(current);

}