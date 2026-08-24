# Outcome writing

## Premise

A command handler returns `Outcome[]` (see [outcomes.md](outcomes.md)) and
never writes to the screen. Something must turn those outcomes into
output, choosing a rendering per outcome type. `IOutcomeIoWriter`
(`KitCli.Commands.Abstractions/Io/IOutcomeIoWriter.cs`) and the eight
built-in implementations beside it are that layer.

## Problem

Outcomes are data of many shapes: a message, a rendered table, a page
size, an exception. Each needs different output, and some need none at
all — a remembered aggregator exists for a later command to query, not
for a user to read.

Putting that rendering in the handler would tie every command to the
console and duplicate the formatting. Putting it in the host loop would
mean one growing type switch over every outcome type in the framework and
in consuming code.

## Solution

Each writer answers two questions:

```csharp
public interface IOutcomeIoWriter
{
    bool CanWriteFor(Outcome outcome);
    void Write(Outcome outcome);
}
```

`CliApp.WriteOutcomes` matches each outcome against the writer list
independently, taking the **first** whose `CanWriteFor` returns `true`:

```csharp
protected void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
{
    foreach (var outcome in outcomes)
    {
        var writer = outcomeIoWriters.FirstOrDefault(w => w.CanWriteFor(outcome));
        writer?.Write(outcome);
    }
}
```

That is the same first-match-wins, registration-order-decides rule used
for command factories (see
[command-registration.md](command-registration.md)) and instruction
argument builders (see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)).

### The built-in writers

`AddCommandAbstractions`
(`CommandsAbstractionsServiceCollectionExtensions.cs`) registers eight, in
this order. Every one takes `ICliIo` (see [cli-io.md](cli-io.md)) and
writes through it, never through `Console`:

| Writer | Claims | Writes |
|---|---|---|
| `NotFoundOutcomeIoWriter` | `CliCommandNotFoundOutcome` | the fixed text "Command Not Found" |
| `OutputOutcomeIoWriter` | `FinalSayOutcome` | its `Something` message |
| `MessageOutcomeIoWriter` | `SayOutcome` | its `Something` message |
| `TableOutcomeIoWriter` | `TableOutcome` | `Table.ToString()`, rendered by `ConsoleTables` |
| `PageSizeOutcomeIoWriter` | `PageSizeOutcome` | `"Page Size: {n}"` |
| `PageNumberOutcomeIoWriter` | `PageNumberOutcome` | `"Page Number: {n}"` |
| `SuggestionOutcomeIoWriter` | `SuggestionOutcome` | `Io.Pause()`, then the name and description |
| `ExceptionOutcomeIoWriter` | `ExceptionOutcome` | the exception's `Message` |

Each `CanWriteFor` is a single type check, so no two claim the same
outcome. First-match-wins therefore decides nothing today; it is the rule
waiting for the first writer that claims a base type or a predicate.

### Outcomes nobody writes

Seven of the fifteen outcome types have no writer:
`RanCliCommandOutcome`, `NextCliCommandOutcome`, `AggregatorOutcome`,
`TableBuilderOutcome`, `AggregatorFilterOutcome`, `ReactionOutcome`, and
`NothingOutcome`.

Six of those carry state for a later command or the state machine, and
have nothing to show. `NothingOutcome` is the deliberate silent ending.
Their silence is the design, not an omission.

### Where the list comes from

`CliAppBuilder.Run` resolves `GetServices<IOutcomeIoWriter>()` once, from
the root provider, and passes the list into the app's `Run`. DI
registration order therefore becomes match order. A caller invoking `Run`
directly may pass any order it likes.

## Constraints & tradeoffs

**An unmatched outcome is dropped in silence.** No fallback writer exists,
unlike `BoolInstructionArgumentBuilder` for arguments. Since seven types
are meant to go unwritten, nothing distinguishes "intentionally silent"
from "forgot a writer". Tracked as
[#18](https://github.com/KitCli/KitCli/issues/18).

**Writers cast unchecked.** Each `Write` casts its argument straight to
the type `CanWriteFor` tested, rather than recovering it with a pattern
match the way `ArtefactFactory` does. Calling `Write` without `CanWriteFor`
throws `InvalidCastException`. Tracked as
[#117](https://github.com/KitCli/KitCli/issues/117).

**Writers are singletons.** They resolve once from the root provider, so a
writer taking a `Scoped` dependency captures one instance for the app's
lifetime. `CliAppBuilder`'s `ValidateScopes` rejects that at startup.

**One writer per outcome, not many.** `WriteOutcomes` stops at the first
match, so two writers cannot each contribute part of one outcome's output.

## Questions & answers

**How do I change how a built-in outcome renders?**
Register your own `IOutcomeIoWriter` for that outcome type ahead of the
built-in one. Registration order decides the match, and
`AddCommandAbstractions` runs from inside `AddCli`, so a writer added
afterwards lands behind the built-ins and never wins.

**Can a writer depend on a `Scoped` service?**
No; see the constraint above. A writer needing I/O takes `ICliIo`. A
writer needing per-run data reads it off the `Outcome` it is handed.

**Why does an `ExceptionOutcome` print in an args app but not an interactive one?**
`TerminalCliApp` rethrows the original exception before `WriteOutcomes`
runs, ending the session (see [cli-app-host.md](cli-app-host.md)).
`ArgsCliApp` does not, so the outcome reaches `ExceptionOutcomeIoWriter`
and prints as the exception's message.

**Why is `ExceptionOutcomeIoWriter` in a file called `CliCommandOutcomeIo.cs`?**
A leftover from an incomplete rename, one of several filename and
type-name mismatches tracked as
[#35](https://github.com/KitCli/KitCli/issues/35). The type is the one to
search for.

## Related concepts

- [outcomes.md](outcomes.md) — the `Outcome` and `OutcomeKind` model these
  writers render, and which outcome each `By...` method appends.
- [cli-io.md](cli-io.md) — `ICliIo`, the seam every writer writes through.
- [cli-app-host.md](cli-app-host.md) — where `WriteOutcomes` is called
  from, once per run, and why `TerminalCliApp` rethrows first.
- [command-registration.md](command-registration.md) — the same
  first-match-wins rule, applied to command factories.
- [0004-first-match-wins-resolution.md](../adr/0004-first-match-wins-resolution.md) —
  why writer resolution, factory resolution, and argument typing all
  follow one rule.
