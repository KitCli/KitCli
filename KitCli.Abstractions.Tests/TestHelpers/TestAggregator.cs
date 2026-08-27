using KitCli.Abstractions.Aggregators;

namespace KitCli.Abstractions.Tests.TestHelpers;

/// <summary>An aggregator that hands its source back unchanged.</summary>
public record TestAggregator : Aggregator<TestAggregate, TestAggregate>
{
    public TestAggregator(params TestAggregate[] aggregates) : base(aggregates)
    {
    }

    protected override IEnumerable<TestAggregate> DoAggregation(IEnumerable<TestAggregate> source)
        => source.ToList();
}
