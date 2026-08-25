using KitCli.Abstractions.Aggregators;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>An aggregator that hands its source back unchanged.</summary>
public record TestAggregator() : Aggregator<TestAggregate, TestAggregate>([])
{
    protected override IEnumerable<TestAggregate> DoAggregation(IEnumerable<TestAggregate> source)
        => source.ToList();
}
