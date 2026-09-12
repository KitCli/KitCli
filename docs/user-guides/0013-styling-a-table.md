# 0013. Styling a table

## What this is for

Choosing how a table's lines are drawn — the frame around it, the dividers
between its columns, the rule under its headers. All nineteen are on this
page, drawn.

```csharp
var table = new ExpenseTableBuilder()
    .WithAggregator(aggregator)
    .WithMap<ExpenseTableMap>()
    .WithPageSize(20)
    .WithPageNumber(1)
    .WithStyle(CliTableStyle.None)
    .Build();
```

Or on a table you built yourself:

```csharp
var table = new Table(columns, rows) { Style = CliTableStyle.None };
```

## Which one to reach for

| You want | Use |
| --- | --- |
| Data somebody reads across | `Ascii`, the default |
| A listing — commands, names, files | `None` |
| Output pasted into an issue or a doc | `Markdown` |
| A modern terminal, drawn lines | `Box` or `RoundedBox` |
| The headings set apart, nothing else | `HeaderLine` |

**A box draws a line between every row. Nothing else does.** That is not
separately settable — a frameless table with lines between its rows reads
as clutter.

## Every style

### A box around everything

```
Ascii                   AsciiGrid               AsciiDoubleHeader

+----------------+      +------+---------+      +------+---------+
| Item | Cost    |      | Item | Cost    |      | Item | Cost    |
|------+---------|      |------+---------|      |======+=========|
| Rent | 1150.00 |      | Rent | 1150.00 |      | Rent | 1150.00 |
|------+---------|      |------+---------|      |------+---------|
| Tea  | 3.20    |      | Tea  | 3.20    |      | Tea  | 3.20    |
+----------------+      +------+---------+      +------+---------+

Box                     RoundedBox              ThickBox

┌──────┬─────────┐      ╭──────┬─────────╮      ┏━━━━━━┳━━━━━━━━━┓
│ Item │ Cost    │      │ Item │ Cost    │      ┃ Item ┃ Cost    ┃
├──────┼─────────┤      ├──────┼─────────┤      ┣━━━━━━╋━━━━━━━━━┫
│ Rent │ 1150.00 │      │ Rent │ 1150.00 │      ┃ Rent ┃ 1150.00 ┃
├──────┼─────────┤      ├──────┼─────────┤      ┣━━━━━━╋━━━━━━━━━┫
│ Tea  │ 3.20    │      │ Tea  │ 3.20    │      ┃ Tea  ┃ 3.20    ┃
└──────┴─────────┘      ╰──────┴─────────╯      ┗━━━━━━┻━━━━━━━━━┛

ThickEdgedBox           ThickHeaderBox          DoubleBox

┏━━━━━━┯━━━━━━━━━┓      ┏━━━━━━┳━━━━━━━━━┓      ╔══════╦═════════╗
┃ Item │ Cost    ┃      ┃ Item ┃ Cost    ┃      ║ Item ║ Cost    ║
┠──────┼─────────┨      ┡━━━━━━╇━━━━━━━━━┩      ╠══════╬═════════╣
┃ Rent │ 1150.00 ┃      │ Rent │ 1150.00 │      ║ Rent ║ 1150.00 ║
┠──────┼─────────┨      ├──────┼─────────┤      ╠══════╬═════════╣
┃ Tea  │ 3.20    ┃      │ Tea  │ 3.20    │      ║ Tea  ║ 3.20    ║
┗━━━━━━┷━━━━━━━━━┛      └──────┴─────────┘      ╚══════╩═════════╝

DoubleEdgedBox

╔══════╤═════════╗
║ Item │ Cost    ║
╟──────┼─────────╢
║ Rent │ 1150.00 ║
╟──────┼─────────╢
║ Tea  │ 3.20    ║
╚══════╧═════════╝
```

### Lines, but no box

```
Dividers                ThickHeaderDividers     DoubleHeaderDividers

  Item │ Cost             Item │ Cost             Item │ Cost
 ──────┼─────────        ━━━━━━┿━━━━━━━━━        ══════╪═════════
  Rent │ 1150.00          Rent │ 1150.00          Rent │ 1150.00
  Tea  │ 3.20             Tea  │ 3.20             Tea  │ 3.20

HeaderLine              ThickHeaderLine         CompactHeaderLine

  Item   Cost             Item   Cost           Item Cost
──────────────────      ━━━━━━━━━━━━━━━━━━      ────────────
  Rent   1150.00          Rent   1150.00        Rent 1150.00
  Tea    3.20             Tea    3.20           Tea  3.20

HeaderAndEdgeLines

──────────────────
  Item   Cost
──────────────────
  Rent   1150.00
  Tea    3.20
──────────────────
```

### Neither

```
Markdown                None

| Item | Cost    |      Item Cost
| ---- | ------- |      Rent 1150.00
| Rent | 1150.00 |      Tea  3.20
| Tea  | 3.20    |
```

## Common mistakes

**Expecting a drawn box on any terminal.** Five styles use nothing but
keyboard characters — `Ascii`, `AsciiGrid`, `AsciiDoubleHeader`, `Markdown`
and `None`. The other fourteen need a font carrying line-drawing
characters. `Ascii` is the default because it needs nothing.

**Reaching for a style to make one column stand out.** The frame is drawn
around the whole table. Nothing styles a single column yet —
[#260](https://github.com/KitCli/KitCli/issues/260).

## Learn more

- [0011-showing-a-table.md](0011-showing-a-table.md) — building the table
  in the first place.
- [../concepts/0009-tables.md](../concepts/0009-tables.md) — how a table
  renders itself.
