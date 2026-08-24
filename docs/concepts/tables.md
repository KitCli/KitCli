# Tables

## Premise

Once an [Aggregator](aggregators.md) has produced a page of rows,
something must turn those typed rows into named columns a user can read,
and must remember enough about the table that a later "next page" command
can rebuild it without redoing the setup. `Table`, `TableMap<T>`, and
`TableBuilder<TSource, TAggregate>` (`KitCli.Abstractions/Tables/`) are
that layer.

## Problem

Turning an arbitrary C# object into named columns needs a mapping from
property to display name, because `.ToString()` on a class tells the user
nothing. Separately, paging a large aggregated list across several
commands needs the *same* builder available later, rather than
reconstructed on every page change.

## Solution

Subclass `TableMap<TAggregate>` (`TableMap.cs`) once per row shape,
calling the protected `Map(x => x.SomeProperty)` in the constructor for
each column:

```csharp
public class TestTableMap : TableMap<TestAggregate>
{
    public TestTableMap()
    {
        Map(x => x.Category).Name("Test Category");
        Map(x => x.TotalCost).Name("Test Total Cost");
    }
}
```

`Map` reads the member from the expression tree, throwing if the
expression is anything but a plain member access, and returns a
`TableColumnMap`. Chain `.Name(...)` onto it to override the displayed
column name; without that, the column takes the C# member's own name.

Subclass `TableBuilder<TSource, TAggregate>` (`TableBuilder.cs`) once per
table shape. The subclass adds no members — it exists to give that
`TSource`/`TAggregate` pairing a distinct type. Configure it fluently:
`WithAggregator(aggregator)`, `WithMap<TMapType>()`, `WithPageSize(n)`,
`WithPageNumber(n)`.

`Build()` demands all four, checked one at a time in that order. It calls
`Aggregator.Aggregate(pageSize, pageNumber)` (see
[aggregators.md](aggregators.md)), reflects over every public property of
`TAggregate`, looks each up in the map's `ColumnMaps`, and returns a
`Table` — a plain `Columns`/`Rows` pair whose `ToString()` renders via the
`ConsoleTables` package.

A `TableBuilder` holds its own aggregator, map, and paging settings, so
remembering the builder is enough for a later command to rebuild the
table without re-supplying any of them. Use `ByRememberingHowToBuildTable`
and `TableBuilderOutcome` (see [outcomes.md](outcomes.md)). From
`KitCli.Playground.Scenarios/TestTableBuilderCliCommand.cs`:

```csharp
// first command
var table = tableBuilder.Build();
return FinishThisCommand()
    .ByShowingTable(table)
    .ByRememberingHowToBuildTable(tableBuilder)
    .EndAsync();

// later "next page" command factory
var tableBuilderArtefact = GetRequiredArtefact<TableBuilder<TestSource, TestAggregate>>();
```

## Constraints & tradeoffs

**A map must cover every public property, not only the ones you want
shown.** `Build()` walks every public instance property of `TAggregate`
and looks each up in `ColumnMaps` through a plain dictionary indexer. A
property with no `Map(...)` call throws `KeyNotFoundException`; unmapped
columns are never skipped. To keep data off a table, keep it off the
aggregate type.

**Every `Build()` precondition throws a bare `System.Exception`.** A
missing aggregator, map, page size, or page number all produce the same
untyped exception, distinguished only by message, though a typed
`CliException` hierarchy exists elsewhere in KitCli. The
`// TODO: This needs cleaning up!` comment on `Build()` marks it; tracked
as [#34](https://github.com/KitCli/KitCli/issues/34).

**`WithMap` is mandatory, with no default.** Nothing falls back to
mapping every public property to a column named after itself, so even a
table with no special column names needs its own `TableMap<TAggregate>`
subclass. A `// TODO: ... maybe default is possible?` comment on `Build()`
marks this as open.

**Column values are always `.ToString()`, with no formatting hook.**
`Build()` calls `.ToString()` on every mapped property's value directly. A
`// TODO: Use the config to determine how to map the value` comment
records that `TableColumnMap` carries no formatting function yet, despite
being the natural place for one.

**`CliTableSortOrder` is dead code.** The enum exists
(`Ascending`/`Descending`), but `TableBuilder`, `TableMap`, and
`TableColumnMap` all ignore it. Tracked as
[#53](https://github.com/KitCli/KitCli/issues/53). Today, sorting comes
only from an `AfterAggregation` function on the `Aggregator` (see
[aggregators.md](aggregators.md)).

## Questions & answers

**Do I need a new `TableBuilder` subclass per table shape, or can I reuse one?**
One per shape. `TableBuilder<TSource, TAggregate>` has no members of its
own; a subclass like `TestTableBuilder : TableBuilder<TestSource, TestAggregate>`
gives that pairing a distinct type, which is also how it gets looked up as
an artefact later.

**What decides the column order?**
`TAggregate`'s public property declaration order, read by
`GetProperties(BindingFlags.Public | BindingFlags.Instance)` in `Build()`.
The order of `Map(...)` calls in `TableMap`'s constructor changes nothing;
those calls decide each column's *name* only.

**Why does a page-change command factory need `GetRequiredArtefact<TableBuilder<TestSource, TestAggregate>>()` instead of something table-specific?**
No table-specific lookup helper exists. A remembered `TableBuilder` is a
generic artefact like any other (see [artefacts.md](artefacts.md)), found
by its closed generic type.

## Related concepts

- [aggregators.md](aggregators.md) — `TableBuilder.WithAggregator(...)`
  supplies the rows a table renders.
- [outcomes.md](outcomes.md) — `TableOutcome` shows a table once;
  `TableBuilderOutcome` and `ByRememberingHowToBuildTable` remember how to
  rebuild one.
- [artefacts.md](artefacts.md) — a remembered `TableBuilder` is retrieved
  like any other artefact, by its closed generic type.
