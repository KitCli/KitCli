# 0010. Workflow run state machine

A run is one arc from an ask to a final outcome, across as many commands as
that takes. `CliWorkflowRun` enforces it as a state machine: an append-only
history of status changes, plus a fixed table of legal from/to pairs.
Anything else throws `ImpossibleStateChangeException`.

| From | To |
|---|---|
| `Created` | `InvalidAsk`, `Running` |
| `Running` | `InvalidAsk`, `Exceptional`, `ReachedReusableOutcome`, `MovePastAsk`, `ReachedFinalOutcome` |
| `ReachedReusableOutcome` | `Running` |
| `MovePastAsk` | `Running`, `InvalidMovePastAsk` |
| `InvalidAsk`, `Exceptional`, `InvalidMovePastAsk`, `ReachedFinalOutcome` | `Finished` |

`ReachedReusableOutcome` and `MovePastAsk` loop back to `Running`. That
loop is how a multi-turn or multi-page run keeps going.

## What decides the next status

`UpdateStateAfterOutcome` reads the **last** outcome only:

- none, or not reusable → `ReachedFinalOutcome`
- a `NextCliCommandOutcome` → `MovePastAsk`, awaiting `MoveToNext()`.
  `MoveToNext()` then builds the command: a `SpecifiedNextCliCommandOutcome`
  is resolved through its factory, and a factory that cannot build it moves
  the run to `Exceptional` like any other failure
- any other reusable outcome → `ReachedReusableOutcome`

`UpdateStateWhenFinished` then checks whether the run ever reached one of
the four terminal statuses and, if so, moves it to `Finished` and disposes
its DI scope. **A legal dead end in the table is not enough** — the code
must call it there, or the run stops one step short and `NextRun()` treats
it as still active.

`NextRun()` reuses the single run short of `Finished`, creating one only
when none exists. `CreateNewRun()` gives each run its own DI scope and the
workflow's cancellation token.

## When an ask leads nowhere

An empty ask, one the validator turns down, and one naming no command are one
case. Parked at a reusable checkpoint the run makes **zero** state changes and
returns the last command's `[CliNextCommandIs]` moves as `SuggestionOutcome`s;
none declared, a silent `NothingOutcome`. Anywhere else all three fail into
`InvalidAsk` and finish. See [0008-suggest-next-commands-attribute.md](../adr/0008-suggest-next-commands-attribute.md).

## Gaps

- One active run is assumed, enforced by `SingleOrDefault` rather than the
  type system. [#42](https://github.com/KitCli/KitCli/issues/42)
- `Runs` and `Changes` grow for the life of the process.
  [#23](https://github.com/KitCli/KitCli/issues/23)
- `FinalSayOutcome`'s message property is named `Something`.
  [#37](https://github.com/KitCli/KitCli/issues/37)
- `ClIWorkflowRunStateStatus` has a typo in its own name.
  [#41](https://github.com/KitCli/KitCli/issues/41)

## See also

[0006-outcomes.md](0006-outcomes.md) · [0002-cli-app-host.md](0002-cli-app-host.md) ·
[0002-di-scope-per-workflow-run.md](../adr/0002-di-scope-per-workflow-run.md)
