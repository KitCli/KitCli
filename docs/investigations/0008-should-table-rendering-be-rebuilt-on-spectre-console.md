# 0008. Should table rendering be rebuilt on Spectre.Console?

- **Status:** In Review
- **Spike:** [#215](https://github.com/KitCli/KitCli/issues/215)
- **Time-box:** 60 minutes
- **Date:** 2026-09-11

## Verdict

New complexity. The swap itself is one ticket — one method changes, nobody's
code breaks — and it shipped as
[#252](https://github.com/KitCli/KitCli/pull/252). What does not follow is
the reason #215 wanted it: that picking a renderer settles the five issues
underneath. Picking one settles exactly one of them, a cell holding a line
break (#214). The other four are still work, and two are not the work #215
described — a row count cannot be built on Spectre at all, and two more were
never the renderer's fault to begin with.

## Recommendation

#215 stays open as the parent, with three pieces under it.

1. **Expose the border style** (#210). `Table.ToString()` fixes it to
   `TableBorder.Ascii`, so the complaint survived the swap intact: a
   different library, still one look nobody can change. Markdown output has
   no replacement at all.
2. **Re-scope #212**, which cannot be built as written.
3. **Leave alignment and formats to
   [#260](https://github.com/KitCli/KitCli/issues/260)**, which #211 and
   #213 folded into. Both were blocked by KitCli, not by the old library.

## What was established

- **The renderer never reaches the public surface.** It appears once, in the
  method that turns a table into text, which is why the swap was one ticket.
- **Spectre has no row count, and no way to choose where a long cell
  breaks.** Both halves of #212 die here. Permanent home:
  [the Spectre.Console page](../technology/spectre-console.md).
- **Alignment and per-column formats were never the renderer's problem.**
  The table builder turns every value into a string before anything draws
  it, so the column's type is gone by then. That is the line #260 has to
  move, and no library choice helps.
- **Japanese and Chinese were already correct.** #215 lists this among the
  things a swap would fix. Both libraries handle it.

## Evidence

Both libraries were drawn side by side in a scratch project — ConsoleTables
2.7.0 against Spectre.Console 0.57.2 — using the stack trace from the
playground command `/test-stack-trace-table`. Forcing the old library's
hidden list of column types by reflection made alignment appear, which is
how that blockage was identified. Everything above is now confirmed by
shipped code in #252.

## Open questions

- Does KitCli count its own rows, or does #212 close unbuilt?
- What replaces markdown output — another border, or KitCli writing
  markdown itself rather than asking a renderer to?

## Out of scope

Colour, prompts, progress bars, and Spectre.Console's own command handling.
Whether KitCli should keep writing to the screen as plain text was not
reached.
