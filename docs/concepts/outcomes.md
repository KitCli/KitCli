# Outcomes

## Premise

After a command runs, its handler must tell KitCli two things: whether
the interaction continues, pauses for another ask, or ends — and what the
user should see. `Outcome` says both at once.

## Problem

Without one shared shape for "what a command produced," every handler
would invent its own return type, and the workflow engine (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)) would
have no consistent way to decide what happens next.

## Solution

Every `CliCommandHandler<T>` returns `Task<Outcome[]>`, built with the
`OutcomeList` fluent builder
(`KitCli.Commands.Abstractions/Outcomes/OutcomeList.cs`):

```csharp
public class MyCommandHandler : CliCommandHandler<MyCommand>
{
    public override Task<Outcome[]> HandleCommand(MyCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByRememberingPageSize(20)
            .ByShowingTable(table)
            .ByFinallySaying("Done.")
            .EndAsync();
}
```

`FinishThisCommand()` returns an empty `OutcomeList`, itself a
`List<Outcome>`. Every `By...` method appends one `Outcome` and returns
`this`, so calls chain. `End()` and `EndAsync()` materialize the list to
`Outcome[]` and `Task<Outcome[]>`.

Every `Outcome` carries an `OutcomeKind` (`Outcome.cs`):

| Kind | Meaning |
|---|---|
| `Anonymous` | No effect on the workflow run — e.g. a plain message. |
| `Reusable` | The run continues, keeping its accumulated context for the next ask. |
| `Final` | Ends the workflow run. |

`Outcome.IsReusable` is `Kind == OutcomeKind.Reusable`. To decide whether
the run continues, pauses, or ends, the workflow engine reads the
**last** outcome in the array and no other — see
[workflow-run-state-machine.md](workflow-run-state-machine.md).

`Kind` decides that and nothing else. Whether a later command can query
an outcome depends on whether an artefact factory claims its type.
`AggregatorFilterOutcome` is `Anonymous` and still becomes a queryable
artefact. See [artefacts.md](artefacts.md).

Built-in outcomes span all three kinds:

| Kind | Outcomes |
|---|---|
| `Anonymous` | `SayOutcome`, `TableOutcome`, `SuggestionOutcome`, `AggregatorFilterOutcome`, `ReactionOutcome` |
| `Reusable` | `PageSizeOutcome`, `PageNumberOutcome`, `AggregatorOutcome`, `TableBuilderOutcome`, `NextCliCommandOutcome`, `RanCliCommandOutcome` |
| `Final` | `FinalSayOutcome`, `CliCommandNotFoundOutcome`, `NothingOutcome`, `ExceptionOutcome` |

`OutcomeList` has a `By...` method for every outcome a handler raises
itself. Three come from the engine instead, so they have none:
`RanCliCommandOutcome` (prepended to every command's outcomes),
`SuggestionOutcome`, and `ExceptionOutcome`.

## Constraints & tradeoffs

**A closed taxonomy of three kinds.** Every new outcome type picks one of
`Anonymous`, `Reusable`, or `Final`; `OutcomeKind` takes no fourth value.
That keeps the engine's next-state decision simple, and leaves no room
for a kind behaving partway between two others.

**Duplication across a returned array is unhandled.** Return two
`TableOutcome`s — or two outcomes of any kind a writer expects once — and
nothing merges or rejects them. The `// TODO: Duplication handling`
comment on `OutcomeList` marks this as an open question. No issue tracks
it, so rely on neither behavior.

## Questions & answers

**How do I make a command end the run?**
Return a `Final`-kind outcome last. `ByFinallySaying(message)` covers the
common case; otherwise derive your own record from
`Outcome(OutcomeKind.Final)`.

**Can the order I add outcomes in change what the run does next?**
Yes. Only the *last* outcome decides the run's next state (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)). Earlier
ones still reach IO writers and artefact factories.

**What happens to an outcome after a handler returns it?**
Two things. An `IOutcomeIoWriter` may display it (see
[outcome-writing.md](outcome-writing.md)), and an artefact factory may
convert it into something later commands can query (see
[artefacts.md](artefacts.md)). The outcome itself is one-shot, scoped to
the command that returned it; the artefact is the queryable form derived
from it.

## Related concepts

- [artefacts.md](artefacts.md) — what a `Reusable` outcome becomes so
  later commands in the same run can query it.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — how
  the last outcome's `Kind` drives the run's state machine.
- [aggregators.md](aggregators.md) — `AggregatorOutcome` and
  `AggregatorFilterOutcome` are two of the built-in outcomes.
- [tables.md](tables.md) — `TableOutcome` and `TableBuilderOutcome` are
  two more.
- [outcome-writing.md](outcome-writing.md) — which outcomes reach the
  screen, which are silently unwritten, and why.
