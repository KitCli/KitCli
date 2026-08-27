# ConsoleTables

`Table.ToString()` renders through
[ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables) 2.7.0 on
`net10.0`, and is the only place in KitCli that touches it. This page
answers "can I change how a table looks" — which of the library's features
KitCli uses, and where each one stops. For how a table is *built*, see
[0009-tables.md](../concepts/0009-tables.md).

```csharp
var table = new ConsoleTable
{
    Options =
    {
        EnableCount = false
    },
    MaxWidth = MaxColumnWidth
};
```

## Output styles

The library renders four styles. KitCli uses `ToString()`, which rules a
divider between every row.

| Style | Method | Divider drawn |
|---|---|---|
| Default | `ToString()` | between every row |
| Alternative | `ToStringAlternative()` | between every row, `+`-jointed |
| MarkDown | `ToMarkDownString()` | under the header only |
| Minimal | `ToMinimalString()` | under the header only, no pipes |

## Settings

| Setting | Library default | KitCli uses | Effect |
|---|---|---|---|
| `MaxWidth` | 40 | `Table.MaxColumnWidth` | per-cell width before text is broken across lines |
| `Options.EnableCount` | `true` | `false` | trailing `Count: n` line |
| `Options.NumberAlignment` | `Left` | default | right-aligns numeric columns |
| `Options.OutputTo` | `Console.Out` | default | where `Write` prints |
| `WordBreakDelimiter` | `' '` | default | character a break prefers |

**`MaxWidth` never makes a table narrower.** Columns are sized from the
full text, so a cell broken at 40 characters sits in a column still wide
enough for all of it, and one row of data reads as two. That is why
`Table.MaxColumnWidth` defaults to `Table.DefaultMaxColumnWidth`, which is
`int.MaxValue`; set a real width and the breaking comes back, but the table
stays the width it was.

**`NumberAlignment` does nothing on a KitCli table.** Alignment reads
`ColumnTypes`, which only `From<T>`, `From(DataTable)`, and
`FromDictionary` populate. KitCli builds rows with `AddColumn`/`AddRow`,
so `ColumnTypes` stays null and every column aligns left.

## Cell values

| Value | Rendered as |
|---|---|
| `string` | itself |
| any other object | `ToString()` |
| `null` | empty cell |
| a string containing `\n` | printed raw, breaking the box |

A row whose cell count differs from the column count throws a bare
`Exception` from `AddRow`, before anything is rendered.

## Gaps

- The `ConsoleTable` is a local variable inside `Table.ToString()`, so
  `MaxColumnWidth` aside, a consumer cannot pick a style or change a
  setting. Tracked as
  [#210](https://github.com/KitCli/KitCli/issues/210) (style) and
  [#212](https://github.com/KitCli/KitCli/issues/212) (row count, word
  break).
- `NumberAlignment` cannot be honoured while rows arrive through
  `AddRow`. Tracked as
  [#211](https://github.com/KitCli/KitCli/issues/211).
- A cell containing a newline breaks the surrounding box, because the
  library measures the whole string as one line. Tracked as
  [#214](https://github.com/KitCli/KitCli/issues/214).
- `From<T>`, `From(DataTable)`, `FromDictionary`, `Formats`, and
  `Write(Format)` are unused; a `TableBuilder` always produces rows.
  `Formats` is what a per-column format would run through, tracked as
  [#213](https://github.com/KitCli/KitCli/issues/213).

## See also

- [Tables](../concepts/0009-tables.md)
- [Artefacts](../concepts/0008-artefacts.md)
