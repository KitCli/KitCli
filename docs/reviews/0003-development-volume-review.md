# 0003. Development volume review

Date: 2026-10-17

**This is a snapshot, not current state.** It compares how much work
landed in KitCli before and after the maintainer started using Claude
Code on 2026-07-22. It measures volume, not pace: lines changed say how
much was written, not how fast finished work shipped.

## Summary

Before Claude, a working day changed about 420 lines. Since July it
changes about 1,450: about 590 lines of code and tests, and 860 of
documentation. Code and tests alone are up by about 40%.

## Methodology

KitCli started inside the
[SpendfulnessCli](https://github.com/joshuaedwardcrowe/SpendfulnessCli)
repository and moved to its own on 2026-01-28. Its history up to then
is the SpendfulnessCli folders that became KitCli: `Cli` and every
`Cli.*` folder except `Cli.Spendfulness.*`, plus `YnabCli.Instructions`
and `YnabProgressConsole.Instructions`, which became `Cli.Instructions`.
From 2026-01-28 it is every commit on KitCli's `main`, leaving out the
commits that copied the code across and the release bot's version
bumps.

A changed line is a line added or deleted. A moved file counts only
its edits. A working day is a date with at least one commit: 49 before
July 2026, 25 since.

## Findings

```mermaid
---
config:
  xyChart:
    height: 640
  themeVariables:
    xyChart:
      plotColorPalette: "#D97757, #6B7280"
---
xychart-beta horizontal
    title "Lines changed each month"
    x-axis ["Feb 2025", "Mar 2025", "Apr 2025", "May 2025", "Jun 2025", "Jul 2025", "Aug 2025", "Sep 2025", "Oct 2025", "Nov 2025", "Dec 2025", "Jan 2026", "Feb 2026", "Mar 2026", "Apr 2026", "May 2026", "Jun 2026", "Jul 2026", "Aug 2026", "Sep 2026", "Oct 2026"]
    y-axis "Lines" 0 --> 27000
    bar [1307, 11, 59, 0, 0, 0, 0, 0, 5802, 5912, 268, 316, 6714, 0, 0, 0, 0, 2621, 25940, 2912, 4745]
    bar [1307, 11, 59, 0, 0, 0, 0, 0, 5802, 5912, 268, 316, 6714, 0, 0, 0, 0, 1798, 1851, 0, 488]
```

Grey is lines from commits written without Claude. Orange is lines from
commits written with it, which carry its `Co-authored-by` trailer.

## Caveats

- **Working days are not hours.** Git records the day of a commit, not
  how long the work took.
- **Documentation is most of the growth.** 21,548 of the 36,218 lines
  changed since July are in `.md` files, against 25 lines before.
- **Lines are not value.** Tidying code lowers its line count, and a
  line of documentation takes different effort from a line of code.
