# Outcomes

## Premise

A command handler needs to tell KitCli two things after it runs: does
the interaction continue, pause for another ask, or end — and what
should the user actually see. `Outcome` is the single object every
command handler returns to say both at once.

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

`FinishThisCommand()` just returns `[]` (an empty `OutcomeList`, itself
a `List<Outcome>`) — every `By...` method appends one `Outcome` and
returns `this`, so calls chain. `.End()` / `.EndAsync()` materialize the
list to `Outcome[]` / `Task<Outcome[]>`.

Every `Outcome` carries an `OutcomeKind` (`Outcome.cs`):

| Kind | Meaning |
|---|---|
| `Anonymous` | No effect on the workflow run — e.g. a plain message. |
| `Reusable` | The run continues; later commands may query this outcome via its artefact (see [artefacts.md](artefacts.md)). |
| `Final` | Ends the workflow run. |

`Outcome.IsReusable` is just `Kind == OutcomeKind.Reusable`. The
workflow engine looks only at the **last** outcome in the returned
array to decide whether the run continues, pauses, or ends — see
[workflow-run-state-machine.md](workflow-run-state-machine.md) for
exactly how.

Built-in outcomes span all three kinds — a few examples: `SayOutcome` /
`TableOutcome` (`Anonymous` — just something to show), `PageSizeOutcome`
/ `AggregatorOutcome` / `NextCliCommandOutcome` (`Reusable` — carry
state a later command factory can query), `FinalSayOutcome` /
`CliCommandNotFoundOutcome` (`Final` — end the run). `OutcomeList` has a
`By...` method for each.

## Constraints & tradeoffs

**A fixed, closed taxonomy of three kinds.** `OutcomeKind` can't be
extended with a fourth value — every new outcome type picks one of
`Anonymous`/`Reusable`/`Final`. This keeps the workflow engine's
next-state decision simple, at the cost of no room for a kind that
behaves partway between two of them.

**Duplication across a returned array is unhandled.** If a handler
returns two `TableOutcome`s, or generally two outcomes of a kind a
writer only expects one of, nothing merges or rejects them — see the
`// TODO: Duplication handling` comment on `OutcomeList` itself. This is
a known, open question left in the source, not documented behavior to
rely on.

## Questions & answers

**How do I make a command end the run?**
Return a `Final`-kind outcome last — `ByFinallySaying(message)` is the
common case, or build your own record deriving `Outcome(OutcomeKind.Final)`.

**Can the order I add outcomes in change what the run does next?**
Yes — only the *last* outcome in the returned array is inspected to
decide the run's next state (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)). Earlier
ones in the same call still reach IO writers and artefact factories,
just not the state decision.

**Where do I look to understand what happens to an outcome after a handler returns it?**
See [artefacts.md](artefacts.md) — an outcome itself is one-shot,
scoped to the command that returned it; an artefact is the derived,
queryable-by-later-commands form built from it.

## Related concepts

- [artefacts.md](artefacts.md) — what a `Reusable` outcome becomes so
  later commands in the same run can query it.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — how
  the last outcome's `Kind` drives the run's state machine.
- [aggregators.md](aggregators.md) — `AggregatorOutcome`/`AggregatorFilterOutcome`
  are two of the built-in outcomes.
- [tables.md](tables.md) — `TableOutcome`/`TableBuilderOutcome` are two more.
