using Bogus;
using KitCli.Abstractions.Aggregators;
using KitCli.Abstractions.Tables;
using KitCli.Commands;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Factories;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Factories;

namespace KitCli.Tests.TestCli.Commands;

public class TestSource
{
    public string Category { get; set; }
    public decimal Cost { get; set; }
}

public record TestAggregate(string Category, decimal TotalCost);

public record TestAggregator(IEnumerable<TestSource> Source) : Aggregator<TestSource, TestAggregate>(Source)
{
    protected override IEnumerable<TestAggregate> DoAggregation(IEnumerable<TestSource> source)
        => source
            .GroupBy(s => s.Category)
            .Select(g => new TestAggregate(g.Key, g.Sum(s => s.Cost)));
}

public record TestAggregatorCliCommand : CliCommand;

public class TestAggregatorCliCommandHandler : CliCommandHandler<TestAggregatorCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestAggregatorCliCommand command, CancellationToken cancellationToken)
    {
        var faker = new Faker<TestSource>()
            .RuleFor(f => f.Category, f => f.Random.Word())
            .RuleFor(f => f.Cost, f => f.Random.Decimal(10, 100));
        
        var source = faker.Generate(1000);
        
        var aggregator = new TestAggregator(source)
            .BeforeAggregation(p => p.Where(s => s.Cost > 50))
            .AfterAggregation(a => a.OrderByDescending(a => a.TotalCost));

        return FinishThisCommand()
            .ByAggregating(aggregator)
            .EndAsync();
    }
}

public record TestAggregatorFilterCliCommand(Aggregator<TestSource, TestAggregate> Aggregator) : CliCommand;

public class TestAggregatorFilterCliCommandFactory : CliCommandFactory<TestAggregatorFilterCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<TestAggregatorCliCommand>();

    public override CliCommand Create()
    {
        var aggregator = GetRequiredAggregatorArtefact<TestSource, TestAggregate>();
        
        return new TestAggregatorFilterCliCommand(aggregator.Value);
    }
}

public class TestAggregatorFilterCliCommandHandler : CliCommandHandler<TestAggregatorFilterCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestAggregatorFilterCliCommand command, CancellationToken cancellationToken)
    {
        var filteredAggregates = command
            .Aggregator
            .BeforeAggregation(p => p.Where(s => s.Cost > 75))
            .AfterAggregation(a => a.OrderByDescending(a => a.TotalCost));
        
        var filter = new AggregatorFilter(
            nameof(TestSource.Cost), 
            "CostAbove", 
            75);

        return FinishThisCommand()
            .ByRememberingFilter(filter)
            .EndAsync();
    }
}

public record TestAggregatorPageCliCommand(Aggregator<TestSource, TestAggregate> Aggregator, int PageSize, int PageNumber) 
    : PagedCliCommand<TestSource, TestAggregate>(Aggregator, PageSize, PageNumber);

public class TestAggregatorPageCliCommandFactory : PagedCliCommandFactory<TestAggregatorPageCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<TestAggregatorFilterCliCommand>();

    public override CliCommand Create()
    {
        var aggregator = GetRequiredAggregatorArtefact<TestSource, TestAggregate>();
        
        var (pageSize, pageNumber) = GetPaging();
        
        return new TestAggregatorPageCliCommand(aggregator.Value, pageSize, pageNumber);
    }
}

public class TestAggregatorPageCliCommandHandler : CliCommandHandler<TestAggregatorPageCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestAggregatorPageCliCommand command, CancellationToken cancellationToken) 
        => FinishThisCommand()
            .ByRememberingPageSize(command.PageSize)
            .ByRememberingPageNumber(command.PageNumber)
            .EndAsync();
}