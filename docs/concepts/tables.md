# Tables

`TableBuilder<TSource, TAggregate>` turns aggregated rows into named
columns, and holds its own aggregator, map, and paging — so remembering the
builder is enough for a later command to rebuild the table without
re-supplying any of it.

```csharp
var tableBuilder = new ExpenseTableBuilder()
    .WithAggregator(aggregator)
    .WithMap<ExpenseTableMap>()
    .WithPageSize(20)
    .WithPageNumber(1);

var table = tableBuilder.Build();
```

`TableMap<TAggregate>` declares the columns, one `Map(x => x.Property)`
call each, chaining `.Name("...")` to override the displayed name.
Subclass `TableBuilder` once per table shape; the subclass adds nothing but
a distinct type, which is how it gets found as an artefact later.

## Map every public property, not just the ones you want shown

`Build()` walks every public instance property of `TAggregate` and looks
each up in the map through a plain dictionary indexer. **An unmapped
property throws `KeyNotFoundException`** rather than hiding that column. To
keep data off a table, keep it off the row type.

Column order follows `TAggregate`'s property declaration order, not the
order of `Map(...)` calls. Those decide each column's *name* only.

## Gaps

- `WithMap` is mandatory; nothing defaults to mapping every property.
- Values are always `.ToString()`; `TableColumnMap` carries no formatting
  function yet.
- Every `Build()` precondition throws a bare `Exception`. Tracked as
  [#34](https://github.com/KitCli/KitCli/issues/34).
- `CliTableSortOrder` is dead code. Tracked as
  [#53](https://github.com/KitCli/KitCli/issues/53). Sorting comes only
  from `AfterAggregation`.

## See also

[aggregators.md](aggregators.md) · [outcomes.md](outcomes.md) ·
[artefacts.md](artefacts.md)
