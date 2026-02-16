using Bogus;
using KitCli.Abstractions.Tables;
using KitCli.Commands.Abstractions;
using KitCli.Commands.Abstractions.Handlers;
using KitCli.Commands.Abstractions.Outcomes;
using KitCli.Commands.Factories;

namespace KitCli.Tests.TestCli.Commands;

public class TestTableMap : TableMap<TestAggregate>
{
    public TestTableMap()
    {
        Map(x => x.Category).Name("Test Category");
        Map(x => x.TotalCost).Name("Test Total Cost");
    }
}

public class TestTableBuilder : TableBuilder<TestSource, TestAggregate>;

public record TestTableBuilderCliCommand : CliCommand;

public class TestTableBuilderCliCommandHandler : CliCommandHandler<TestTableBuilderCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestTableBuilderCliCommand command, CancellationToken cancellationToken)
    {
        var faker = new Faker<TestSource>()
            .RuleFor(f => f.Category, f => f.Random.Word())
            .RuleFor(f => f.Cost, f => f.Random.Decimal(10, 100));
        
        var source = faker.Generate(1000);
        
        var aggregator = new TestAggregator(source)
            .BeforeAggregation(p => p.Where(ts => ts.Cost > 50))
            .AfterAggregation(a => a.OrderByDescending(ta => ta.TotalCost));

        var tableBuilder = new TestTableBuilder()
            .WithAggregator(aggregator)
            .WithMap<TestTableMap>()
            .WithPageSize(2)
            .WithPageNumber(1);
        
        var table = tableBuilder.Build();
        
        return FinishThisCommand()
            .ByShowingTable(table)
            .ByRememberingPageSize(2)
            .ByRememberingPageNumber(1)
            .ByRememberingHowToBuildTable(tableBuilder)
            .EndAsync();
    }
}

public record TestTableBuilderPageChangeCliCommand(TableBuilder<TestSource, TestAggregate> TableBuilder, int PageSize, int PageNumber) : CliCommand;

public class TestTableBuilderPageChangeCliCommandFactory : PagedCliCommandFactory<TestTableBuilderPageChangeCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<TestTableBuilderCliCommand>();

    public override CliCommand Create()
    {
        var tableBuilderArtefact = GetRequiredArtefact<TableBuilder<TestSource, TestAggregate>>();

        var (pageSize, pageNumber) = GetPaging();
        
        return new TestTableBuilderPageChangeCliCommand(
            tableBuilderArtefact.Value,
            pageSize,
            pageNumber
        );
    }
}

public class TestTableBuilderPageChangeCliCommandHandler : CliCommandHandler<TestTableBuilderPageChangeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestTableBuilderPageChangeCliCommand command, CancellationToken cancellationToken)
    {
        command.TableBuilder
            .WithPageSize(command.PageSize)
            .WithPageNumber(command.PageNumber);

        var table = command.TableBuilder.Build();
        
        return FinishThisCommand()
            .ByShowingTable(table)
            .ByRememberingPageSize(command.PageSize)
            .ByRememberingPageNumber(command.PageNumber)
            .EndAsync();
    }
}