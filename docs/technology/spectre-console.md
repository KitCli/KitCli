# Spectre.Console

`Table.ToString()` renders through
[Spectre.Console](https://github.com/spectreconsole/spectre.console) 0.57.2 on
`net10.0`, and is the only place in KitCli that touches it. This page answers
"can I change how a table looks" — which of the library's features KitCli uses,
and where each one stops. For how a table is *built*, see
[0009-tables.md](../concepts/0009-tables.md). Why the library changed is in
[ADR 0017](../adr/0017-render-tables-with-spectre-console.md).

```csharp
var table = new Spectre.Console.Table
{
    Border = TableBorder.Ascii,
    ShowRowSeparators = true
};
```

## Output styles

The library ships nineteen table borders. KitCli uses `Ascii`, which is the
closest to what it printed before, and turns row separators back on so a table
still rules a line between rows.

| Border | Looks like | Divider drawn |
|---|---|---|
| `Ascii` | `+---+`, `|` | header, plus between rows when asked |
| `Square` | `┌───┐`, `│` | header, plus between rows when asked |
| `Markdown` | `| --- |` | under the header only |
| `None` | no border | none |

Every border draws a line under the header. `ShowRowSeparators` adds one
between rows; without it, rows sit flush against each other.

## Settings

| Setting | Library default | KitCli uses | Effect |
|---|---|---|---|
| `Table.Border` | `Square` | `Ascii` | which characters draw the box |
| `Table.ShowRowSeparators` | `false` | `true` | a line between rows |
| `Table.ShowHeaders` | `true` | default | whether the header row prints |
| `Table.Width` | unset | unset | fixed width for the whole table |
| `Table.Title` / `Caption` | unset | unset | free text above and below |
| `TableColumn.Width` | unset | `Table.MaxColumnWidth` | see below |
| `TableColumn.Alignment` | left | default | left, centre or right |
| `TableColumn.NoWrap` | `false` | default | never break this column |
| `Profile.Width` | the terminal's | default | see below |

**A column width is exact, not a maximum.** Set 40 and a short value is padded
out to 40 rather than left alone. So `Table.MaxColumnWidth` is applied only to
columns whose widest line already exceeds it — which restores its meaning of
"wrap here", without stretching the narrow columns.

**The library measures the real terminal even when writing to a string.** It
cannot tell that the output is not a console, and falls back to 80 columns only
when there is no terminal at all. KitCli leaves that alone, so a table is sized
to fit the window it is about to be printed to — which is the point: a value too
long for its column wraps inside the column instead of running off the side.

## Cell values

| Value | Rendered as |
|---|---|
| `string` | itself |
| any other object | `ToString()` |
| `null` | empty cell |
| a string containing `\n` | as many lines, inside its own row |
| a string containing `[` | itself |

**Square brackets would otherwise be read as markup.** Handed a raw string, the
library treats `[red]` as a colour instruction and throws on anything it cannot
resolve — which a generic type name like ``List`1[System.String]`` triggers.
KitCli wraps every value in a `Text`, which is printed literally.

A row with more cells than there are columns throws an
`InvalidOperationException` from `AddRow`. A row with fewer is accepted, and the
missing cells render empty.

## Gaps

- A consumer cannot pick a border or change a setting; the `Table` is a local
  variable inside `Table.ToString()`. Tracked as
  [#210](https://github.com/KitCli/KitCli/issues/210).
- **No row count.** The library has no equivalent of a trailing `Count: n`
  line — only `Caption`, which is free text a caller composes. Tracked as
  [#212](https://github.com/KitCli/KitCli/issues/212).
- **No choice of where a long cell breaks.** The library wraps at word
  boundaries with nothing to configure, so a column of file paths cannot be
  told to prefer `/`. Also [#212](https://github.com/KitCli/KitCli/issues/212).
- **No per-column formatting.** `TableColumn.Alignment` is available but
  unused, because `TableBuilder` flattens every value to text before the
  renderer sees it. Tracked as
  [#211](https://github.com/KitCli/KitCli/issues/211) and
  [#213](https://github.com/KitCli/KitCli/issues/213).
