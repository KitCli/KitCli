# CliApp host loop

## Premise

[workflow-run-state-machine.md](workflow-run-state-machine.md) covers how
one `CliWorkflowRun` gets from an ask to an outcome. Something must sit
outside that run: source the ask, decide whether to fetch a new one or
continue an existing run, hand the outcomes to a writer, and give a
consuming application hooks into all of it without editing the loop.
`CliApp` (`KitCli/CliApp.cs`) is that shared shell. Two subclasses in
`KitCli/` — `TerminalCliApp` and `ArgsCliApp` — supply the two ways of
driving it.

An app picks its mode by which subclass it extends, and cannot switch at
runtime. See
[0005-args-driven-cli-app.md](../adr/0005-args-driven-cli-app.md) for why
a compile-time choice beats a flag `Run` branches on.

## Problem

A host loop must:

- Run until the workflow stops, without knowing *why* it stopped. A
  one-shot invocation must instead stop itself after exactly one run,
  without touching workflow-run state.
- Tell `MovePastAsk` runs — paging, multi-step continuations — from runs
  needing a fresh ask, and drive each differently.
- Let a consuming application observe each stage — session start, run
  created, run started, run complete, session end — without forking the
  loop per application.
- Answer a cancelled session by stopping cleanly, leaving no partial state
  behind.

## Solution

### `CliApp`: the shared shell

`CliApp` owns what both modes need: the `Workflow` and `Io` references,
`SetUpEventHandlers` (the `Io.OnCancel` wiring, see
[cli-io.md](cli-io.md)), `WriteOutcomes` (see
[outcome-writing.md](outcome-writing.md)), and the six lifecycle hooks
below. It declares no `Run` method and no loop; each subclass supplies
that.

### `TerminalCliApp`: the interactive loop

`TerminalCliApp.Run(List<IOutcomeIoWriter> outcomeIoWriters, string[]? args = null)`
starts an interactive session. It ignores `args`; the parameter exists so
`CliAppBuilder` can call either subclass's `Run` the same way:

```csharp
OnSessionStart();
Io.Pause();
SetUpEventHandlers();

while (Workflow.Status != CliWorkflowStatus.Stopped)
{
    var run = Workflow.NextRun();
    OnRunCreated(run);
    var outcomes = await ExecuteRunOperation(run);
    WriteOutcomes(outcomes, outcomeIoWriters);
    OnRunComplete(run, outcomes);
    Io.Pause();
}

OnSessionEnd(Workflow.Runs);
```

The loop condition reads `Status` and never the `CancellationToken`.
`Workflow.InterruptCurrentRun()`, which a Ctrl+C reaches through the
`ICliIo` wiring (see [cli-io.md](cli-io.md)), flips `Status` to `Stopped`
as it requests cancellation. `Status` alone therefore answers "should the
loop keep going" accurately, whether the session ended by `/exit` or by
Ctrl+C.

`Workflow.NextRun()` (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)) hands back
either the one in-progress run or a fresh one. `TerminalCliApp` reacts to
what it gets; it never chooses.

### `ArgsCliApp`: one-shot from process args

`ArgsCliApp.Run(List<IOutcomeIoWriter> outcomeIoWriters, string[] args)`
runs a single command without a prompt, letting a KitCli app be invoked as
`myapp /command --flag value`. It joins `args` into one ask, feeds that
through the same `RespondToAsk` pipeline an interactive ask uses, then
stops the workflow:

```csharp
OnSessionStart();
Io.Pause();
SetUpEventHandlers();

var run = Workflow.NextRun();
OnRunCreated(run);
var ask = string.Join(" ", args);
var runTask = run.RespondToAsk(ask);
OnRunStarted(run, ask);
var outcomes = await runTask;
WriteOutcomes(outcomes, outcomeIoWriters);
OnRunComplete(run, outcomes);
Workflow.Stop();
OnSessionEnd(Workflow.Runs);
```

As in `TerminalCliApp`, `OnRunStarted` fires after `RespondToAsk` is
called but before it is awaited, so a hook can show a "working…" indicator
while the run executes.

Three points deserve attention:

- It calls `Workflow.Stop()`, the same public method
  `ExitCliCommandHandler` calls, rather than reaching into `run.State` to
  force `ReachedFinalOutcome`. `CliApp` and its subclasses never mutate a
  run's state; only `CliWorkflowRun` does (see
  [workflow-run-state-machine.md](workflow-run-state-machine.md)).
- It calls `Workflow.Stop()` unconditionally, whatever state the run
  reached. Should the ask resolve to a multi-step command needing
  `MoveToNext()`, `ArgsCliApp` stops after the first step. One-shot
  invocation means one command today, not an automated sequence.
- It passes no `CancellationToken` into `RespondToAsk`, and needs none.
  `Workflow.NextRun()` already handed the run its token at construction,
  so cancelling mid-command reaches this run exactly as it reaches
  `TerminalCliApp`'s.

### `CliAppBuilder`: choosing and starting the app

`CliAppBuilder.Run(string[]? args)` picks the subclass's `Run` from the
concrete `CliApp` resolved out of DI and whether `args` arrived. An
`ArgsCliApp` asked to run with no args throws a specific
`ArgumentException` rather than doing nothing or failing opaquely.

It builds the service provider first, with `ValidateScopes` and
`ValidateOnBuild` both on, then resolves the `CliApp` and the
`IOutcomeIoWriter` list once, from the root provider, before any run
starts. Anything reached from there is a singleton for the app's lifetime,
while command handlers get per-run instances from the run's own scope (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)). The two
validations enforce that boundary: a singleton depending on a `Scoped`
service fails at startup, naming both types, rather than silently
capturing one instance and holding it.

### Ask vs. move-past-ask (`TerminalCliApp` only)

`TerminalCliApp`'s private `ExecuteRunOperation` checks the run it was
handed for a recorded `MovePastAsk` state change.

If it finds one, it calls `run.MoveToNext()` and asks for no input: a
prior outcome already queued the work, such as "show the next page."

If it finds none, it calls `Io.AskAsync(Workflow.CancellationToken)` and
passes the result to `run.RespondToAsk(ask)`. Neither `MoveToNext` nor
`RespondToAsk` takes a `CancellationToken`; `CliWorkflow` gave the run one
at construction.

`OnRunStarted` and `OnMovingPastAsk` fire *after* the run's task starts
but *before* it is awaited, matching `ExecuteCommand` and `RespondToAsk`
inside the run. A hook can show a "working…" indicator while the run
executes, rather than after it finishes.

Once either call returns, `ExecuteRunOperation` looks through the outcomes
for an `ExceptionOutcome`, the marker an `Exceptional` run carries (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)). Finding
one, it rethrows the original exception through `ExceptionDispatchInfo`,
preserving the stack trace and ending the whole loop. That is deliberate.
`Exceptional` means a command failed in a way the app never accounted for,
so it ends the session loudly — unlike `InvalidAsk` (a typo or unknown
command) and `ReachedFinalOutcome`, which both let the loop continue.

Only `TerminalCliApp` rethrows, and it rethrows before `WriteOutcomes`
runs. In `ArgsCliApp` the same outcome reaches a writer and prints (see
[outcome-writing.md](outcome-writing.md)).

### Lifecycle hooks

Six `protected virtual` no-op hooks let a consuming application observe
the loop without overriding `Run`: `OnSessionStart`, `OnRunCreated`,
`OnRunStarted`, `OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd`. Each
fires at a fixed point above. None alters control flow; they exist for
side effects — progress indicators, logging, telemetry.

In an interactive session every hook can fire many times. In a one-shot
invocation five fire once each, and `OnMovingPastAsk` never fires, because
an args app never continues a run past its ask.

## Constraints & tradeoffs

**Run-mode is a compile-time choice.** The subclass an app extends decides
args-driven or terminal-driven for the life of that class. No single
`CliApp` supports both depending on how it was launched. See
[0005-args-driven-cli-app.md](../adr/0005-args-driven-cli-app.md) for the
alternatives weighed.

**Neither `Run` is sealed.** A consuming application may override the whole
loop instead of using the hooks, but then owns the `NextRun` and
`MovePastAsk` routing and the outcome writing. The hooks exist to make
that unnecessary.

**A command handler ignoring its `CancellationToken` still runs to
completion on Ctrl+C.** Cooperative cancellation interrupts only handlers
that check the token `ISender.Send` passes them. See
[0006-cooperative-cancellation.md](../adr/0006-cooperative-cancellation.md).

**No hook can prevent or redirect a transition.** All six return `void`
and receive no way to signal "don't continue" or "run something else."
Flow control stays in `Run` and the run's state machine, leaving one place
to read to know what happens next.

## Questions & answers

**How does `TerminalCliApp` know whether to call `AskAsync()` or `MoveToNext()`?**
It reads the run's state history for a prior `MovePastAsk` change. The run
is the source of truth; `TerminalCliApp` tracks nothing itself (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)).
`ArgsCliApp` never faces the question, calling `RespondToAsk` once and
`MoveToNext` never.

**Why call `Io.Pause()` before the loop as well as after every iteration?**
So an implementation gets a pause point before the first prompt and after
every later one, with no special case for the first iteration.

**Which hook should do expensive setup a run depends on?**
None of them. `OnRunStarted` and `OnMovingPastAsk` run while the run
executes, and the rest cannot delay it either. Setup a command depends on
belongs in the command's factory or handler.

**Can I run several workflows from one app?**
No. `CliAppBuilder` resolves one `CliApp`, which holds one `ICliWorkflow`,
and `NextRun()` assumes a single active run (see
[workflow-run-state-machine.md](workflow-run-state-machine.md)).

## Related concepts

- [workflow-run-state-machine.md](workflow-run-state-machine.md) — what
  `Workflow.NextRun()`, `RespondToAsk`, and `MoveToNext` do. This doc
  covers only what drives them from outside.
- [cli-io.md](cli-io.md) — the `ICliIo` seam behind `Io.AskAsync` and
  `Io.Pause`, and how a Ctrl+C reaches `SetUpEventHandlers`.
- [outcome-writing.md](outcome-writing.md) — what `WriteOutcomes` does
  with the array each run returns.
