# 0008. Should table rendering be rebuilt on Spectre.Console?

- **Status:** In Review
- **Spike:** [#215](https://github.com/KitCli/KitCli/issues/215)
- **Time-box:** 60 minutes
- **Date:** 2026-09-10

## Verdict

No new complexity. Swapping the library that draws KitCli's tables changes one
method, breaks nobody's code, and turns four of the five issues behind #215
into settings — which is the outcome #215 hoped for.

Nothing grew. There is no prerequisite: the pre-1.0 risk of the new library is
contained by pinning a version, and the package everything depends on already
carries a drawing library, so swapping which one does not create the problem
[#14](https://github.com/KitCli/KitCli/issues/14) describes. The one decision
record needed is the one #215 already said it would need.

The swap is worth making, but not for the reason #215 gave. It set its own
bar: output that merely looks better does not justify swapping a
dependency, while a library that takes work off KitCli does. Spectre.Console
fails that bar — it has no opinion about how objects turn into rows, and that
stays KitCli's own code. The bar was the wrong one. Four issues close outright,
which is justification on its own:

- **A cell containing line breaks** ([#214]) stays inside its own row.
- **Border styles** ([#210]) — nineteen of them, chosen per table.
- **Numbers lined up on the right** ([#211]) — a setting on the column.
- **A row count** ([#212]) — trivial under either library.

Lining numbers up is the one worth explaining, because today it cannot be done
at all. ConsoleTables only right-aligns a column when it knows that column
holds numbers, and it only learns that when a table is built from a list of one
known type. KitCli builds its rows from a column map instead, and the place
that knowledge would sit cannot be filled from outside the library — so it is
always empty. Spectre needs no such knowledge: right-alignment is a setting on
the column, and it works on plain text.

That leaves the fifth issue, which the swap does nothing for. [#213] wants a
column to choose how its values look — two decimal places, say. KitCli turns
every value into text before anything else happens, using no format at all, and
it needs to use the column's chosen format instead. That is KitCli's own code
under either library. It is the one thing this spike found that #215 did not
expect, and it is not new: it was already recorded as a gap in
[the tables concept doc](../concepts/0009-tables.md) before the spike began.
It is small, and no consumer can see the difference — a table's rows already
hold plain objects, and a single method fills them.

## Recommendation

Close #215 and open a fresh delivery ticket for the swap.

1. **Swap the drawing library, and write a decision record.** One method
   changes. No public name or signature moves, so what changes is what users
   see printed, not the code anyone writes. This settles [#214], [#210],
   [#211] and [#212] together.
2. **Build [#213] separately.** It needs the table builder to apply a column's
   chosen format instead of plain text, which is KitCli's own code. Nothing
   blocks it and it blocks nothing.

Do not patch [#214] inside the old library first. It would be about ten lines,
thrown away by step 1, and it would not even look right — a stack trace would
still read as several rows ruled off from each other, because the old library
draws a divider after every line it prints.

Do not wait for [#14](https://github.com/KitCli/KitCli/issues/14) either. Moving
table drawing out of the package everything depends on is still the right thing,
but it is not a prerequisite, and its milestone is one issue of eight done with
no due date. Waiting would leave [#214] broken indefinitely.

## What was established

- **The old library never reaches the public surface.** It appears once, inside
  the method that turns a table into text. Every public name a consumer touches
  survives a swap untouched. Two things still change: the text that comes out,
  and what `Table.MaxColumnWidth` means — today it wraps text without making
  the column any narrower, under Spectre it genuinely caps the column's width.
- **Spectre has to be told how wide a table may be.** The old library makes a
  table as wide as its contents and leaves the terminal to cope. Spectre fits a
  table to the console — and there is no console when producing text, so a
  width has to be chosen. A large number preserves today's behaviour.
- **The old library handles Japanese and Chinese text correctly.** #215 lists
  this among the things a swap would fix. It does not need fixing. Permanent
  home: [the ConsoleTables page](../technology/console-tables.md).
- **Spectre.Console is heavily used despite its version number.** 56.8 million
  downloads against ConsoleTables' 12.6 million, 11,611 stars against 1,007,
  MIT licensed, committed to daily. Work titled "Preparations for the 1.0
  release" landed in August 2024 and 1.0 still has not shipped, so the version
  reads as a habit rather than a warning.
- **Its updates are not reliably breaking.** Of the last three, 0.55.0 was
  breaking and said so plainly; 0.56.0 and 0.57.0 were fixes and additions. All
  three only added to table drawing, which is the whole of what KitCli uses.
- **Nobody upstream will fix [#214] for us.** ConsoleTables' author closed the
  same request with "I don't think this library supports it", recommended
  Spectre.Console, and described his own project as "a fun experiment".

## Evidence

Versions: ConsoleTables 2.7.0, Spectre.Console 0.57.2. Everything below was run
in a scratch project against those two.

Both drawings used the stack trace from the playground command
`/test-stack-trace-table`. Under Spectre its three lines sit inside one row and
every line of the box measures the same width. Under the old library the second
and third lines print raw and spill past the border.

Alignment was checked four ways. The old library lines numbers up when a table
is built from a list of a known type, and does not when rows are added one at a
time — whether the values are real numbers or text — because the column's type
is left empty and cannot be filled from outside the library. Reaching in and
filling it makes the alignment appear, which is how we know that is the
blockage. Spectre lines up a column of plain text from a setting on the column
alone.

Japanese text was checked by drawing a table of it and measuring every line:
the old library pads each row to the same width on screen.

Download counts came from NuGet's search API; stars, activity and release
contents from the GitHub CLI. The upstream position is in ConsoleTables issues
[#60](https://github.com/khalidabuhakmeh/ConsoleTables/issues/60),
[#71](https://github.com/khalidabuhakmeh/ConsoleTables/issues/71) and
[#88](https://github.com/khalidabuhakmeh/ConsoleTables/issues/88).

## Open questions

- Which border style replaces today's look? Every table an app prints changes
  appearance, so whoever picks it needs to say so in a release note.
- What replaces markdown output? KitCli does not offer it today, so nothing is
  lost yet, but [#210] wants it and Spectre's markdown border pads its result
  with blank lines.
- Does `Table.MaxColumnWidth` keep its name once it means something different?
- Should the part of KitCli that writes to the screen keep taking plain text,
  or should the drawing library own the console? The time-box stopped first.

## Out of scope

Colour, prompts, progress bars, and Spectre.Console's own command handling —
KitCli has its own. How a column's chosen format should be applied was not
designed here, only shown to be small and independent of the library.

[#210]: https://github.com/KitCli/KitCli/issues/210
[#211]: https://github.com/KitCli/KitCli/issues/211
[#212]: https://github.com/KitCli/KitCli/issues/212
[#213]: https://github.com/KitCli/KitCli/issues/213
[#214]: https://github.com/KitCli/KitCli/issues/214
