# 0009. Tables

A table is columns, rows, and the frame drawn around them. `Table` holds
all three and renders itself as fixed-width text. `TableBuilder<TSource,
TAggregate>` produces one, keeping the aggregator, the column map and the
page together so a later command can rebuild the same table from nothing.

```csharp
var table = new ExpenseTableBuilder()
    .WithAggregator(aggregator)
    .WithMap<ExpenseTableMap>()
    .WithPageSize(20)
    .WithPageNumber(1)
    .Build();
```

## Every property is looked up, not only the mapped ones

`Build()` walks every public property of the row type and reads the map
through a plain dictionary indexer:

```csharp
public record ExpenseRow(string Category, decimal TotalCost, int ReceiptId);

public class ExpenseTableMap : TableMap<ExpenseRow>
{
    public ExpenseTableMap() => Map(x => x.Category).Name("Category");
}
```

```
System.Collections.Generic.KeyNotFoundException:
The given key 'System.Decimal TotalCost' was not present in the dictionary.
```

**An unmapped property throws rather than hiding that column.** Keep data
off a table by keeping it off the row type. Column order follows the row
type's declaration order; the `Map(...)` calls decide names only.

## The style is chosen, the row lines follow

`Style`, or `WithStyle(...)` on the builder, picks one of nineteen
`CliTableStyle` values — all of them drawn in
[../user-guides/0013-styling-a-table.md](../user-guides/0013-styling-a-table.md).
`Ascii` is the default and the look every table had before the setting
existed.

**Lines between rows are not separately settable.** The ten styles that
draw a box draw them:

```
+------------------------+      Category  Total Cost
| Category  | Total Cost |      Groceries 412.80
|-----------+------------|      Rent      1150.00
| Groceries | 412.80     |
|-----------+------------|
| Rent      | 1150.00    |
+------------------------+
```

The other nine leave the rows to run on, because lines between the rows of
a table with no frame around it read as clutter.

## Sizing

The table is measured against the console it is about to print to — eighty
columns when there is none — so a long value wraps inside its own column
rather than running off the side. `MaxColumnWidth` holds one column
narrower still. A value with line breaks in it takes as many lines as it
has, inside its own row, and square brackets print literally.

## Gaps

- `WithMap` is mandatory; nothing defaults to mapping every property.
- A page size and number are mandatory too, so a table that never pages
  still declares one.
- Values are always `.ToString()`. A column cannot set its alignment, its
  format or its colour — [#260](https://github.com/KitCli/KitCli/issues/260).
- Every `Build()` precondition throws a bare `Exception` —
  [#34](https://github.com/KitCli/KitCli/issues/34).
- `CliTableSortOrder` is dead code —
  [#53](https://github.com/KitCli/KitCli/issues/53). Sorting comes only from
  `AfterAggregation`.

## See also

[0007-aggregators.md](0007-aggregators.md) · [0006-outcomes.md](0006-outcomes.md) ·
[0008-artefacts.md](0008-artefacts.md) ·
[../user-guides/0011-showing-a-table.md](../user-guides/0011-showing-a-table.md) ·
[Spectre.Console](../technology/spectre-console.md)
