# 0011. Showing a paged table

## What this is for

Showing a list as a table — named columns, filtering, sorting, and a "next
page" that skips the setup a second time — is common enough that KitCli
gives it a dedicated pipeline. This is how to wire one up.

This assumes your registry calls `AddArtefactFactoriesForAssembly` (see
[0004-creating-a-registry.md](0004-creating-a-registry.md)). Without it the
"next page" step cannot find the remembered builder.

## How to do it

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

**3. A table builder subclass**, giving the pairing a distinct type — which
is how a later command finds it again:

```csharp
public class ExpenseTableBuilder : TableBuilder<Expense, ExpenseRow>;
```

**4. The command that puts it together**, remembering the builder so
somebody can page it:

```csharp
var aggregator = new ExpenseAggregator(expenses)
    .BeforeAggregation(source => source.Where(e => e.Cost > 0))
    .AfterAggregation(rows => rows.OrderByDescending(r => r.TotalCost));

var tableBuilder = new ExpenseTableBuilder()
    .WithAggregator(aggregator)
    .WithMap<ExpenseTableMap>()
    .WithPageSize(20)
    .WithPageNumber(1);

return FinishThisCommand()
    .ByShowingTable(tableBuilder.Build())
    .ByRememberingPageSize(20)
    .ByRememberingPageNumber(1)
    .ByRememberingHowToBuildTable(tableBuilder)
    .EndAsync();
```

`BeforeAggregation` runs on the raw source, ahead of your aggregation
logic; `AfterAggregation` runs on the aggregated rows.

### Letting the user page through it

The remembered builder comes back as an artefact, so the next command
re-supplies neither aggregator nor column map:

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
```

Its handler sets `WithPageSize`/`WithPageNumber` on that builder, calls
`Build()` again, and remembers the new paging.

`PagedCliCommandFactory<T>`'s `GetPaging()` reads page size and number from
the current ask, falls back to the remembered artefact, then defaults to 20
and 1. So `/next-page --pageNumber 3` and a bare `/next-page`, reusing the
page you were on, both work through one factory.

The argument names are `pageNumber` and `pageSize`, camelCase.
`--page-number` is a different argument, and is ignored.

## Common mistakes

**Skipping `WithMap<TMap>()`.** No default maps every property for you.
`Build()` throws when no map is set.

**Mapping only the columns you want shown.** `Build()` looks up *every*
public property of the row type, so an unmapped one throws
`KeyNotFoundException` rather than hiding that column. To keep something
off the table, keep it off the row type.

**Sorting or filtering after `Build()`.** A built `Table` is a plain
columns-and-rows pair with no pipeline left. Sort and filter on the
aggregator.

**Rebuilding the aggregator and map by hand for "next page".**
`ByRememberingHowToBuildTable` exists so a later command reconstructs none
of it.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  the general pattern behind `ByRememberingHowToBuildTable`.
- [../concepts/0007-aggregators.md](../concepts/0007-aggregators.md) — the full
  aggregation pipeline beneath step 1.
- [../concepts/0009-tables.md](../concepts/0009-tables.md) — how a map and a
  builder become a rendered `Table`, plus the known gaps — no default
  column mapping, no value formatting — to weigh before relying on either.
