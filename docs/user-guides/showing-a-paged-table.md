# Showing a paged table

## What this is for

Showing a list as a table — named columns, filtering, sorting, and "next
page" support that skips the setup a second time — is common enough that
KitCli gives it a dedicated pipeline. This is how to wire one up.

## How to do it

Four pieces, then a command that builds and shows the table.

This assumes your registry calls `AddArtefactFactoriesForAssembly` (see
[creating-a-registry.md](creating-a-registry.md)). That call registers the
artefact factories for the aggregator and table builder below; without it,
the "next page" step cannot find the remembered builder.

**1. An aggregator**, turning raw source data into the rows the table
displays:

```csharp
public record ExpenseRow(string Category, decimal TotalCost);

public record ExpenseAggregator(IEnumerable<Expense> Source)
    : Aggregator<Expense, ExpenseRow>(Source)
{
    protected override IEnumerable<ExpenseRow> DoAggregation(IEnumerable<Expense> source)
        => source
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseRow(g.Key, g.Sum(e => e.Cost)));
}
```

**2. A column map**, naming the columns from the row shape:

```csharp
public class ExpenseTableMap : TableMap<ExpenseRow>
{
    public ExpenseTableMap()
    {
        Map(x => x.Category).Name("Category");
        Map(x => x.TotalCost).Name("Total Cost");
    }
}
```

**3. A table builder subclass**, giving the source and row-type pairing a
distinct type:

```csharp
public class ExpenseTableBuilder : TableBuilder<Expense, ExpenseRow>;
```

**4. The command that puts it together:**

```csharp
public class ListExpensesCliCommandHandler : CliCommandHandler<ListExpensesCliCommand>
{
    public override Task<Outcome[]> HandleCommand(ListExpensesCliCommand command, CancellationToken ct)
    {
        var aggregator = new ExpenseAggregator(expenses)
            .BeforeAggregation(source => source.Where(e => e.Cost > 0))
            .AfterAggregation(rows => rows.OrderByDescending(r => r.TotalCost));

        var tableBuilder = new ExpenseTableBuilder()
            .WithAggregator(aggregator)
            .WithMap<ExpenseTableMap>()
            .WithPageSize(20)
            .WithPageNumber(1);

        var table = tableBuilder.Build();

        return FinishThisCommand()
            .ByShowingTable(table)
            .ByRememberingPageSize(20)
            .ByRememberingPageNumber(1)
            .ByRememberingHowToBuildTable(tableBuilder)
            .EndAsync();
    }
}
```

`BeforeAggregation` runs on the raw source, ahead of your aggregation
logic; `AfterAggregation` runs on the aggregated rows. Reach for whichever
matches what you are filtering or sorting.

### Letting the user page through it

`ByRememberingHowToBuildTable` makes "next page" possible without
re-supplying the aggregator or column map. A later command reads the
remembered builder back as an artefact:

```csharp
public class NextExpensePageCliCommandFactory : PagedCliCommandFactory<NextExpensePageCliCommand>
{
    public override bool CanCreateWhen() => LastCommandWas<ListExpensesCliCommand>();

    public override CliCommand Create()
    {
        var tableBuilder = GetRequiredArtefact<TableBuilder<Expense, ExpenseRow>>();
        var (pageSize, pageNumber) = GetPaging();

        return new NextExpensePageCliCommand(tableBuilder.Value, pageSize, pageNumber);
    }
}

public class NextExpensePageCliCommandHandler : CliCommandHandler<NextExpensePageCliCommand>
{
    public override Task<Outcome[]> HandleCommand(NextExpensePageCliCommand command, CancellationToken ct)
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
```

`PagedCliCommandFactory<T>`'s `GetPaging()` reads page size and number
from the current ask's arguments, falls back to the remembered artefact,
then defaults to 20 and 1. So `/next-page --pageNumber 3` and a bare
`/next-page`, reusing the page you were on, both work through one factory.

The argument names are `pageNumber` and `pageSize`, camelCase, as declared
on `PagedCliCommand<,>.ArgumentNames`. `--page-number` is a different
argument, and is ignored.

## Common mistakes

**Skipping `WithMap<TMap>()`.** No default maps every property for you.
`Build()` throws when no map is set, even for a table with no special
column names.

**Mapping only the columns you want shown.** `Build()` looks up *every*
public property of the row type, so an unmapped one throws
`KeyNotFoundException` rather than hiding that column. Give `ExpenseRow` a
`RawTotal` property and `ExpenseTableMap` needs a `Map(...)` call for it
too. To keep something off the table, keep it off the row type.

**Sorting or filtering after `Build()`.** A built `Table` is a plain
`Columns` and `Rows` pair with no pipeline left. Filter and sort on the
aggregator, through `BeforeAggregation` or `AfterAggregation`, before
calling `Build()`.

**Rebuilding the aggregator and map by hand for "next page".**
`ByRememberingHowToBuildTable` exists so a later command reconstructs
none of it. Passing the aggregator and map into a second command's
constructor redoes what the remembered artefact already gives you.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the general pattern behind `ByRememberingHowToBuildTable`.
- [docs/concepts/aggregators.md](../concepts/aggregators.md) — the full
  `Aggregator<TSource, TAggregate>` pipeline beneath step 1:
  `BeforeAggregation`, `DoAggregation`, `AfterAggregation`, paging.
- [docs/concepts/tables.md](../concepts/tables.md) — how `TableMap` and
  `TableBuilder` turn aggregated rows into a rendered `Table`, plus the
  known gaps — no default column mapping, no value formatting hook — to
  weigh before relying on either.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — how a
  remembered `TableBuilder` is retrieved by a later command, the same
  way any other artefact is.
