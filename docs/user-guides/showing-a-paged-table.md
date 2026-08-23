# Showing a paged table

## What this is for

Showing a list as a table — with named columns, filtering, sorting,
and "next page" support that doesn't redo all the setup — is a common
enough shape that KitCli has a dedicated pipeline for it. This is how
to wire one up.

## How to do it

Four pieces, then a command that builds and shows the table.

**1. An aggregator** — turns your raw source data into the rows the
table will display:

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

**2. A column map** — names the columns from the aggregated row shape:

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

**3. A table builder subclass** — exists purely to give the
source/row-type pairing a distinct type:

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

`BeforeAggregation` runs on the raw source before your aggregation
logic; `AfterAggregation` runs on the aggregated rows after it —
reach for whichever matches what you're filtering or sorting.

### Letting the user page through it

`ByRememberingHowToBuildTable` is what makes "next page" possible
without re-supplying the aggregator or column map — a later command
just reads the remembered builder back as an artefact:

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

`PagedCliCommandFactory<T>`'s `GetPaging()` helper reads page
size/number from the current ask's arguments first, falling back to
the remembered artefact, then a default of 20/1 — so `/next-page
--page-number 3` and a bare `/next-page` (reusing whatever page you
were already on) both work through the same factory.

## Common mistakes

**Skipping `WithMap<TMap>()`.** There's no default that maps every
property automatically — `Build()` throws if a map hasn't been set,
even for a table with no special column names.

**Sorting or filtering after `Build()` instead of via
`BeforeAggregation`/`AfterAggregation`.** A built `Table` is a plain
`Columns`/`Rows` pair with no further pipeline — all filtering and
sorting has to happen on the aggregator before you call `Build()`.

**Rebuilding the aggregator and map by hand for "next page" instead
of remembering the `TableBuilder`.** The whole point of
`ByRememberingHowToBuildTable` is that a later command doesn't need
to reconstruct any of this — if you find yourself passing the
aggregator and map into a second command's constructor, you're
redoing what the remembered artefact already gives you.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  the general pattern `ByRememberingHowToBuildTable` is one instance
  of.
- [docs/concepts/aggregators.md](../concepts/aggregators.md) — the
  full `Aggregator<TSource, TAggregate>` pipeline (`BeforeAggregation`
  /`DoAggregation`/`AfterAggregation`/paging) underneath step 1.
- [docs/concepts/tables.md](../concepts/tables.md) — how `TableMap`
  and `TableBuilder` actually turn aggregated rows into a rendered
  `Table`, and known gaps (no default column mapping, no value
  formatting hook) worth knowing about before you rely on either.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — how a
  remembered `TableBuilder` is retrieved by a later command, the same
  way any other artefact is.
