# 0008. Should table rendering be rebuilt on Spectre.Console?

- **Status:** In Review
- **Spike:** [#215](https://github.com/KitCli/KitCli/issues/215)
- **Time-box:** 60 minutes
- **Date:** 2026-09-10

## Verdict

No new complexity. Swapping the library that draws KitCli's tables changes one
method, breaks nobody's code, and settles four of the five issues behind #215:
line breaks in a cell (#214), border styles (#210), numbers lined up on the
right (#211), and a row count (#212).

It fails the bar #215 set for itself — Spectre.Console has no opinion about how
objects turn into rows, so the table builder is unchanged either way. Four
issues closing outright is the justification instead.

## Recommendation

Close #215 and open a fresh delivery ticket for the swap, with a decision
record. Build #213 separately whenever: it needs the table builder to apply a
column's chosen format, which no library change helps with, and is already a
known gap in [the tables concept doc](../concepts/0009-tables.md).

Two options were rejected. Patching #214 inside the old library is throwaway
work that still looks wrong, because that library rules off every line it
prints. Waiting for [#14](https://github.com/KitCli/KitCli/issues/14) is worse:
its milestone is one issue of eight done, with no due date.

## What was established

- **The old library never reaches the public surface.** It appears once, inside
  the method that turns a table into text. What changes is the printed text, and
  `Table.MaxColumnWidth`: today it wraps without narrowing, Spectre caps width.
- **Numbers cannot be right-aligned today at all.** The old library aligns only
  a column it knows holds numbers, and learns that only from a path KitCli
  cannot take. Spectre needs no such knowledge — alignment is a column setting.
- **The old library handles Japanese and Chinese correctly.** #215 lists this
  among the things a swap would fix. It does not. Permanent home:
  [the Spectre.Console page](../technology/spectre-console.md).
- **Spectre is heavily used despite its version.** 56.8 million downloads
  against 12.6 million, MIT, committed to daily, 1.0 "coming" since August 2024.
  One of its last three updates broke things; all three only added to table
  drawing. The old library's author closed the same request as #214 with "I
  don't think this library supports it", and recommended Spectre.Console.

## Evidence

Run in a scratch project against ConsoleTables 2.7.0 and Spectre.Console 0.57.2,
using the stack trace from the playground command `/test-stack-trace-table`.
Alignment was checked four ways; forcing the old library's hidden list of column
types by reflection makes the alignment appear, which is how the blockage was
identified. Counts came from NuGet's search API and the GitHub CLI, the upstream
position from ConsoleTables issues 60, 71 and 88.

## Open questions

- How wide may a table be? There is no console to fit to when producing text.
- Which border style replaces today's look, and what replaces markdown output?

## Out of scope

Colour, prompts, progress bars, and Spectre.Console's own command handling.
Whether KitCli should keep writing to the screen as plain text was not reached.
