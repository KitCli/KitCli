# CliApp host loop

## Premise

Everything documented in
[workflow-run-state-machine.md](workflow-run-state-machine.md) explains how
one `CliWorkflowRun` gets from an ask to an outcome. Something still has to
sit outside that: ask the user for input, decide when to fetch a new ask
versus continue an existing run (paging), write outcomes back out, and give
a consuming application hooks into all of that without editing the loop
itself. `CliApp` (`KitCli/CliApp.cs`) is that outer shell.

## Problem

A host loop needs to:

- Keep running until the workflow stops, without the loop itself knowing
  *why* it stopped.
- Tell `MovePastAsk` runs (paging, multi-step continuations) apart from
  runs that need a fresh ask, and drive each one differently.
- Turn an `Outcome[]` into actual output, when different outcome types need
  different rendering.
- Let a consuming application observe or react at each stage (session
  start, run created, run started, run complete, session end) without
  forking the loop per application.
- React to a user-cancelled session (e.g. Ctrl+C) by stopping cleanly
  rather than leaving the process in a partial state.

## Solution

### The loop

`CliApp.Run(List<IOutcomeIoWriter> outcomeIoWriters)` is the entry point:

```csharp
while (_workflow.Status != CliWorkflowStatus.Stopped)
{
    var run = _workflow.NextRun();
    OnRunCreated(run);
    var outcomes = await ExecuteRunOperation(run);
    WriteOutcomes(outcomes, outcomeIoWriters);
    OnRunComplete(run, outcomes);
    Io.Pause();
}
```

`_workflow.NextRun()` (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)) hands back
either the one in-progress run or a freshly created one — `CliApp` doesn't
decide which; it only reacts to what it gets.

### Ask vs. move-past-ask

`ExecuteRunOperation` checks whether the run it was just handed already has
a `MovePastAsk` state change recorded. If so, it calls `run.MoveToNext()`
directly — no `Io.Ask()` call, because the run has queued-up work from a
prior outcome (e.g. "show the next page") and doesn't need fresh input. If
not, it calls `Io.Ask()` for a new ask and passes that into
`run.RespondToAsk(ask)`.

`OnRunStarted`/`OnMovingPastAsk` fire *after* the run's task has been
started but *before* it's awaited — same reasoning as
`ExecuteCommand`/`RespondToAsk` in the workflow run itself: a hook here can
show a "working…" indicator concurrently with the run actually executing,
rather than only after it finishes.

### Writing outcomes

```csharp
private void WriteOutcomes(Outcome[] outcomes, List<IOutcomeIoWriter> outcomeIoWriters)
{
    foreach (var outcome in outcomes)
    {
        var writer = outcomeIoWriters.FirstOrDefault(w => w.CanWriteFor(outcome));
        writer?.Write(outcome);
    }
}
```

Each outcome in the array is matched against the writer list independently,
taking the **first** `IOutcomeIoWriter` whose `CanWriteFor` returns `true`
— the same first-match-wins, registration-order-decides pattern used for
instruction argument builders (see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)) and
command/artefact factory resolution (see
[command-registration.md](command-registration.md)). If nothing matches,
that outcome is silently not written — there's no default/fallback writer,
unlike `BoolInstructionArgumentBuilder`'s role for arguments.

### The `ICliIo` abstraction

`CliApp` never touches `Console` directly. `ICliIo`
(`KitCli.Abstractions/Io/ICliIo.cs`) is the seam:

```csharp
public interface ICliIo
{
    string? Ask();
    void Pause();
    void Say(string something);
    void SetTitle(string title);
    void OnCancel(Action cancel);
}
```

`Run` calls `Io.Pause()` once before the loop starts and again after every
iteration — giving a host implementation a place to, e.g., wait for a
keypress between commands. `SetUpEventHandlers` wires `Io.OnCancel` once,
at the top of `Run`, to stop the workflow, fire `OnSessionEnd`, and exit
the process — the only place `CliApp` calls `Environment.Exit` itself.

### Lifecycle hooks

Six `protected virtual` no-op hooks let a consuming application observe the
loop without overriding `Run` itself: `OnSessionStart`, `OnRunCreated`,
`OnRunStarted`, `OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd`. Each
fires at a fixed point in the loop above; none of them can alter control
flow — they're for side effects only (progress indicators, logging,
telemetry), matching the same reasoning as `CliWorkflowRunState`'s history
being the only place state actually changes.

## Constraints & tradeoffs

**`Run` is not sealed.** A consuming application *can* override the whole
loop instead of using the hooks, but doing so takes on reimplementing
`NextRun`/`MovePastAsk` routing and outcome-writing correctly — the hooks
exist so that's rarely necessary.

**`Environment.Exit` inside `OnCancel`.** Cancelling a session bypasses the
loop's own `while` condition entirely and terminates the process directly.
A host that wants a graceful non-process-exiting cancellation path (e.g.
returning control to an embedding application) isn't served by this today.

**No hook can prevent or redirect a transition.** All six are `void`; none
receive a way to signal "don't continue" or "run something else instead."
This mirrors the reasoning already documented for
`CliWorkflowRun`'s hooks in the SpendfulnessCli-era design (unstructured
mutation of flow control makes the loop harder to reason about) — kept
here for the same reason, not by omission.

## Questions & answers

**How does `CliApp` know whether to call `Ask()` or `MoveToNext()`?**
It checks the run's own state history for a prior `MovePastAsk` change —
`CliApp` never tracks this itself; the run is the source of truth (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)).

**What happens if two `IOutcomeIoWriter`s both claim the same outcome?**
The first one in `outcomeIoWriters` (as passed into `Run`) wins; the rest
are never consulted for that outcome. There's no registration-based
ordering here the way there is for instruction argument builders or
artefact factories — the caller of `Run` controls the list's order
directly.

**Why does `Io.Pause()` get called both before the loop and after every
iteration, rather than just once per iteration?**
So an implementation gets a pause point both before the very first prompt
and after every subsequent one — without needing special-casing for "is
this the first iteration."

## Related concepts

- [workflow-run-state-machine.md](workflow-run-state-machine.md) — what
  `_workflow.NextRun()`, `RespondToAsk`, and `MoveToNext` actually do;
  this doc only covers what drives them from the outside.
- [command-registration.md](command-registration.md) — the same
  first-match-wins resolution pattern used here for `IOutcomeIoWriter`.
- [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md) —
  where `Io.Ask()`'s return value ends up being parsed.
