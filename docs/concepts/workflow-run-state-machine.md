# Workflow run state machine

## Premise

Running a KitCli app means repeatedly turning user input ("asks") into
commands and outcomes, possibly across many separate command
executions, without losing track of what's already happened. `ICliWorkflow`
(`CliWorkflow.cs`) holds a list of `ICliWorkflowRun`s; each
`CliWorkflowRun` (`Run/CliWorkflowRun.cs`) is its own small state machine
tracking one execution from "ask received" through to "finished."

## Problem

At any point, the host (`CliApp`) needs to know: is this ask valid? Did
the command just run finish the whole interaction outright, does it want
another ask, or does it want to run again *without* a fresh ask (e.g.
paging through a list)? What happens if parsing or validation fails, or
the command handler throws? All of that needs one enforced set of legal
transitions — not scattered booleans, where a run left in an
inconsistent state (e.g. simultaneously "reached its final outcome" and
"still running") becomes a silent bug rather than an exception. This is
exactly the class of bug this state machine exists to prevent: two real
production bugs — the run never reaching `Finished` after certain error
paths, and a command-lookup exception that was structurally uncatchable
— existed here until recently, precisely because the finishing logic
wasn't consistently applied. Both are fixed on `main` today; see git
history for `CliWorkflowRun.cs` if you want the specifics.

## Solution

### The two levels

`CliWorkflow` is the top-level object: a `Runs` list plus a `Status`
(`Started`/`Stopped`). `NextRun()` does **not** always create a new run —
it reuses the single run in `Runs` that hasn't yet reached
`ReachedFinalOutcome`, and only calls `CreateNewRun()` if none exists:

```csharp
public ICliWorkflowRun NextRun()
{
    var lastRunNotHavingReachedFinalOutcome = Runs
        .SingleOrDefault(run => !run.State.WasChangedTo(ClIWorkflowRunStateStatus.ReachedFinalOutcome));

    return lastRunNotHavingReachedFinalOutcome ?? CreateNewRun();
}
```

Each `CliWorkflowRun` exposes exactly two entry points a caller can call:
`RespondToAsk(string? ask)` — parse, validate, and run a command from
fresh user input — and `MoveToNext()` — re-enter a command a prior
outcome already queued up, with no new input (used for things like
"show the next page").

### The state machine itself

`ICliWorkflowRunState`/`CliWorkflowRunState` (`Run/State/CliWorkflowRunState.cs`)
is the actual finite state machine: an append-only
`List<ICliWorkflowRunStateChange>` history, plus a fixed table of
`PossibleCliWorkflowRunStateChange` pairs (`IfStartedAt` → `CanMoveTo`).
`ChangeTo(...)` looks up the most recently reached status, checks the
table, and throws `ImpossibleStateChangeException` if the transition
isn't listed — the table is the entire contract for what's legal:

| From | To |
|---|---|
| `Created` | `InvalidAsk`, `Running` |
| `Running` | `InvalidAsk`, `Exceptional`, `ReachedReusableOutcome`, `MovePastAsk`, `ReachedFinalOutcome` |
| `InvalidAsk` | `Finished` |
| `Exceptional` | `Finished` |
| `ReachedReusableOutcome` | `Running` |
| `MovePastAsk` | `Running`, `InvalidMovePastAsk` |
| `InvalidMovePastAsk` | `Finished` |
| `ReachedFinalOutcome` | `Finished` |

Note that `ReachedReusableOutcome` and `MovePastAsk` both loop back to
`Running` rather than going straight to `Finished` — that loop is how a
multi-turn or multi-page run keeps going without ending the state
machine.

### Walking the actual flow

`RespondToAsk`:
- Empty/null ask → `ChangeTo(InvalidAsk)`, return early.
- Parses via `IInstructionParser` (see
  [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md));
  if `IInstructionValidator` accepts it → `ChangeTo(Running, instruction)`,
  otherwise → `ChangeTo(InvalidAsk)`, return early.
- Looks up a command via `ICliWorkflowCommandProvider.GetCommand(...)`.
  If no factory exists for the instruction (`NoCommandGeneratorException`)
  → `ChangeTo(InvalidAsk)`, then explicitly calls `UpdateStateWhenFinished()`
  itself, because this catch fires *before* `ExecuteCommand` is ever
  reached — there's no `ExecuteCommand`-owned `finally` block to fall
  through to here.
- Otherwise, hands off to `ExecuteCommand`.

`ExecuteCommand`:
- Runs the command through `ISender.Send`, prepends a
  `RanCliCommandOutcome` marker to the returned outcomes, publishes any
  `ReactionOutcome`s via `IPublisher`, then calls `UpdateStateAfterOutcome`
  with the full outcome array.
- Any exception anywhere in that → `ChangeTo(Exceptional)`.
- A `finally` block always calls `UpdateStateWhenFinished()` on the way
  out, whichever path was taken.

`UpdateStateAfterOutcome` looks only at the **last** outcome returned
(see [outcome-artefact-pipeline.md](outcome-artefact-pipeline.md) for
what outcomes and their `Kind` mean) to decide the next status: no
outcomes, or the last one isn't reusable → `ReachedFinalOutcome`; the
last one is a `NextCliCommandOutcome` → `MovePastAsk` (the run is now
waiting on a `MoveToNext()` call, not a fresh ask); any other reusable
outcome → `ReachedReusableOutcome`.

`UpdateStateWhenFinished` checks whether the run has *ever* reached one
of the three "run over" statuses (`ReachedFinalOutcome`, `InvalidAsk`,
`Exceptional`) and, if so, transitions to `Finished`. Because it checks
the run's whole history rather than just the most recent change, it's
safe to call from more than one place (both `RespondToAsk`'s
`NoCommandGeneratorException` catch and `ExecuteCommand`'s `finally`
call it) without double-finishing a run that's already finished.

`MoveToNext` is only valid if some outcome in the run's history so far
was a `NextCliCommandOutcome` (`IsValidMovePastAsk`); it takes the
**last** such outcome and re-executes its `NextCommand` through the same
`ExecuteCommand` path.

## Constraints & tradeoffs

**An explicit transition table as data, over inline conditionals.**
Illegal transitions throw immediately with a clear message pointing at
exactly which from/to pair was rejected — at the cost of the table
needing to be kept in sync by hand every time a status or transition is
added. Nothing generates it from the code that actually performs
transitions.

**At most one active run per workflow, enforced by `SingleOrDefault`, not the type system.**
`CliWorkflow.NextRun()` assumes there is never more than one run in
`Runs` that hasn't reached `ReachedFinalOutcome`. If that invariant is
ever violated, `SingleOrDefault` throws a generic
`InvalidOperationException`, not a domain-specific one — this is a
known, tracked gap, not a deliberately chosen error type.

**Run and state-change history are never evicted.** Both
`CliWorkflow.Runs` and `CliWorkflowRunState.Changes` simply grow for the
life of the process. Fine for a short CLI invocation; worth knowing
about before reusing a long-lived `CliWorkflow` in, say, a long-running
host process.

## Questions & answers

**What decides whether a run continues after a command, versus ending?**
The *last* outcome the handler returned — see
[outcome-artefact-pipeline.md](outcome-artefact-pipeline.md) for what
outcomes and `OutcomeKind` mean. This doc only covers how that decision
maps onto run state, not the outcome/artefact model itself.

**Can two runs be "in progress" on the same workflow at once?**
Not by design — see the constraint above. `NextRun()` always resumes the
one incomplete run if one exists, rather than starting a second one
alongside it.

**Why does `RespondToAsk` sometimes call `UpdateStateWhenFinished()` directly instead of just letting `ExecuteCommand` handle it?**
Because the `NoCommandGeneratorException` case never reaches
`ExecuteCommand` at all — there's no command to execute, so there's no
`finally` block downstream to rely on. `RespondToAsk` finishes the run
itself, right where the failure that ends it actually happened.
