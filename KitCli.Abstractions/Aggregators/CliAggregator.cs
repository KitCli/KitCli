namespace KitCli.Abstractions.Aggregators;

public abstract class CliAggregator<TAggregation>
{
    public abstract TAggregation Aggregate();
}