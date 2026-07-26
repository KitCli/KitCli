# Tables

## Premise

Once an [Aggregator](aggregators.md) has produced a page of rows,
something has to turn those typed rows into named columns a user can
read, and remember enough about how the table was built that a later
"next page" command can rebuild it without redoing all the setup.
`Table`, `TableMap<T>`, and `TableBuilder<TSource, TAggregate>`
(`KitCli.Abstractions/Tables/`) are that layer.

## Problem

Turning an arbitrary C# object into named columns needs some mapping
from property to display name, since a raw `.ToString()` on a class
isn't useful. Separately, paging through a large aggregated list across
multiple commands needs the *same* builder to be re-invokable later,
rather than reconstructed from scratch on every page change.

## Solution

`TableMap<TAggregate>` (`TableMap.cs`) is subclassed once per row shape,
calling the protected `Map(x => x.SomeProperty)` for each column to
include, in the constructor:

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

`Map` reads the member from the expression tree (throws if the
expression isn't a plain member access) and returns a `TableColumnMap`
you can chain `.Name(...)` onto to override the displayed column name —
otherwise it defaults to the C# member's own name.

`TableBuilder<TSource, TAggregate>` (`TableBuilder.cs`) is also
subclassed once per table shape — the subclass has no members of its
own; it exists purely to give that `TSource`/`TAggregate` pairing a
distinct type. Fluent setup: `WithAggregator(aggregator)`,
`WithMap<TMapType>()`, `WithPageSize(n)`, `WithPageNumber(n)`.

`Build()` requires all four to have been set, checked one at a time
(aggregator, then map, then page size, then page number). It calls
`Aggregator.Aggregate(pageSize, pageNumber)` (see
[aggregators.md](aggregators.md)), reads every public property
`TAggregate` has via reflection, looks each one up in the map's
`ColumnMaps`, and returns a `Table` — a plain `Columns`/`Rows` pair
whose `ToString()` renders via the `ConsoleTables` package.

Because a `TableBuilder` holds its aggregator, map, and paging settings
internally, remembering the whole builder (`TableBuilderOutcome`/
`ByRememberingHowToBuildTable`, see [outcomes.md](outcomes.md)) is
enough for a later command to rebuild without re-supplying the
aggregator or map. A real example, from
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

**Every `Build()` precondition throws a bare `System.Exception`, one at
a time.** Missing an aggregator, map, page size, or page number all
produce the same untyped exception, distinguished only by message —
there's no typed exception hierarchy here, unlike elsewhere in KitCli
(`CliException` and friends). The `// TODO: This needs cleaning up!`
comment on `Build()` itself flags this as known, not intentional.

**`WithMap` is mandatory, with no default.** There's no fallback that
maps every public property to a column named after itself — even a
table with no special column names still needs a `TableMap<TAggregate>`
subclass written for it. A `// TODO: ... maybe default is possible?`
comment on `Build()` flags this as a known, open question.

**Column values are always `.ToString()`, with no formatting hook.**
`Build()` calls `.ToString()` on every mapped property's value directly
— a `// TODO: Use the config to determine how to map the value` comment
flags that `TableColumnMap` doesn't actually carry a formatting
function yet, despite being the natural place for one.

**`CliTableSortOrder` is unused.** The enum exists
(`Ascending`/`Descending`) but nothing in `TableBuilder`, `TableMap`, or
`TableColumnMap` references it — dead code, tracked as
[#53](https://github.com/KitCli/KitCli/issues/53). Sorting is only
actually available today via a raw `AfterAggregation` function on the
`Aggregator` (see [aggregators.md](aggregators.md)).

## Questions & answers

**Do I need a new `TableBuilder` subclass per table shape, or can I reuse one?**
Per shape — `TableBuilder<TSource, TAggregate>` has no members of its
own; a subclass like `TestTableBuilder : TableBuilder<TestSource, TestAggregate>`
exists purely to give that pairing a distinct type, which also matters
for how it gets looked up as an artefact later.

**What decides the column order?**
`TAggregate`'s public property declaration order, via
`GetProperties(BindingFlags.Public | BindingFlags.Instance)` reflection
in `Build()` — not the order `Map(...)` calls appear in in `TableMap`'s
constructor.

**Why does a page-change command factory need `GetRequiredArtefact<TableBuilder<TestSource, TestAggregate>>()` instead of something table-specific?**
There's no table-specific artefact lookup helper — a remembered
`TableBuilder` is just a generic artefact like any other (see
[artefacts.md](artefacts.md)), looked up by its closed generic type.

## Related concepts

- [aggregators.md](aggregators.md) — `TableBuilder.WithAggregator(...)`
  is what actually produces the rows a table renders.
- [outcomes.md](outcomes.md) — `TableOutcome` shows a table once;
  `TableBuilderOutcome`/`ByRememberingHowToBuildTable` remembers how to
  rebuild one.
- [artefacts.md](artefacts.md) — a remembered `TableBuilder` is
  retrieved the same way any other artefact is, by its closed generic
  type.
