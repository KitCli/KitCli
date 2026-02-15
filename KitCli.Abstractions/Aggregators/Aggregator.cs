namespace KitCli.Abstractions.Aggregators;

public abstract class Aggregator<TAggregate>
{
    public abstract TAggregate Aggregate();
}