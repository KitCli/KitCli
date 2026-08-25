using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;

namespace KitCli.Commands.Abstractions.Tests.TestHelpers;

/// <summary>A row of whatever a test is aggregating.</summary>
public record TestAggregate(string Name);

/// <summary>An aggregator that hands its source back unchanged.</summary>
public record TestAggregator() : Aggregator<TestAggregate, TestAggregate>([])
{
    protected override IEnumerable<TestAggregate> DoAggregation(IEnumerable<TestAggregate> source)
        => source.ToList();
}

/// <summary>A table builder with no configuration beyond what a test gives it.</summary>
public class TestTableBuilder : TableBuilder<TestAggregate, TestAggregate>;

/// <summary>A reaction a handler can publish as a side effect.</summary>
public record TestCliCommandReaction(string Because) : CliCommandReaction;

/// <summary>A command a handler can chain to.</summary>
public record TestNextCliCommand : CliCommand;
