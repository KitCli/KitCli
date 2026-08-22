# 0006. Drive Ctrl+C shutdown with a CancellationToken, not Environment.Exit

Status: Proposed
Date: 2026-08-22

## Context

`CliApp.SetUpEventHandlers` wired `Io.OnCancel` to `Console.CancelKeyPress`,
which .NET fires on its own thread pool thread, concurrently with whatever
the main loop thread was doing mid-`ExecuteCommand`:

```csharp
Io.OnCancel(() =>
{
    _workflow.Stop();
    OnSessionEnd(_workflow.Runs);
    Environment.Exit(exitCode: 0);
});
```

Two problems followed from this, reported in #74:

- `_workflow.Stop()` ran on the cancel thread while the main thread could
  still be using services resolved from the in-flight run's DI scope — a
  cross-thread dispose race, on top of the backstop sweep already added for
  [#71](0002-di-scope-per-workflow-run.md).
- `Environment.Exit` terminates the process immediately, skipping any
  `finally`/`Dispose()` that hasn't already run synchronously. Scoped
  services can have real cleanup side effects (flushing writes,
  committing/rolling back a transaction, releasing a lock) — skipping that
  is a correctness issue, not just a resource leak.

## Decision

Replace the abrupt exit with cooperative cancellation, owned by
`ICliWorkflow` rather than `CliApp`:

- `CliWorkflow` owns a `CancellationTokenSource` and exposes it as
  `CancellationToken CancellationToken { get; }` plus a
  `void InterruptCurrentRun()` method that cancels it. This mirrors how
  `CliWorkflow` already owns the per-run `IServiceScope` (see
  [0002](0002-di-scope-per-workflow-run.md)) and hands it to
  `CliWorkflowRun`'s constructor — the token is handed in the same way,
  as one more constructor argument alongside `serviceScope`, not threaded
  through `RespondToAsk`/`MoveToNext` on every call.
- `CliApp.SetUpEventHandlers` wires `Io.OnCancel` to
  `Workflow.InterruptCurrentRun` — nothing else. No `Stop()`, no
  `OnSessionEnd`, no `Environment.Exit` on the cancel thread itself, and
  `CliApp` itself holds no cancellation state of its own.
- `CliIo.OnCancel`'s `Console.CancelKeyPress` handler sets `e.Cancel = true`,
  suppressing .NET's own default abrupt termination so the app drives
  shutdown instead.
- `CliWorkflow.CreateNewRun` passes its `CancellationToken` into
  `CliWorkflowRun`'s constructor, which stores it privately and passes it to
  `_sender.Send(command, _cancellationToken)` inside `ExecuteCommand` — an
  in-flight run can observe cancellation and unwind through its normal
  `catch`/`finally` → `Dispose()` path instead of being disposed out from
  under it by another thread. `ICliWorkflowRun.RespondToAsk`/`MoveToNext`
  keep their original signatures; cancellation is ambient to the run, not a
  parameter callers must supply.
- `Console.ReadLine()` isn't natively cancellable. `ICliIo.Ask()` became
  `AskAsync(CancellationToken)`, which starts a background
  `Task.Run(Console.ReadLine)` and awaits it via `.WaitAsync(cancellationToken)`,
  catching the resulting `OperationCanceledException` and returning `null`
  if cancellation wins — abandoning the still-blocked read rather than
  waiting on it. That's safe because the abandoned task runs on a thread
  pool (background) thread, which doesn't keep the process alive once the
  main thread returns. `Task.WaitAsync` (.NET 6+) was chosen over the older
  `Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, cancellationToken))`
  idiom specifically because the latter leaks: the losing `Task.Delay`
  never completes on its own (there's no timer for an infinite delay) and
  is never disposed by `WhenAny`, so it stays registered against the token
  forever — see [dotnet/runtime#46603](https://github.com/dotnet/runtime/issues/46603).
  Since `Workflow.CancellationToken` lives for the whole session,
  `WhenAny` would have accumulated one such leaked registration per ask.
  `WaitAsync` handles its own registration/disposal correctly, with none of
  that. `TerminalCliApp` passes `Workflow.CancellationToken` into this
  call — the only place a `CliApp` subclass reads the token directly, since
  sourcing an ask is a `CliApp`-level concern that `CliWorkflow` has no
  part in.
- `InterruptCurrentRun()` also calls `Stop()` internally, so
  `TerminalCliApp.Run`'s `while` loop needs no change at all from before
  this ADR — it still just checks `Workflow.Status != CliWorkflowStatus
  .Stopped`. Cancellation and a normal `/exit` both end up flipping the
  exact same `Status` flag, so `Status` alone is always an accurate,
  immediately-visible answer to "should the loop keep going," regardless of
  which of the two ended the session. `ArgsCliApp` needs no changes at all
  for cancellation to reach its single run — `CliWorkflowRun` already
  picked up the token when `CliWorkflow.NextRun()` created it.

`Stop()` and `InterruptCurrentRun()` are still two separate methods, but
the relationship between them is one-directional, not symmetric:
*cancelling always stops* (`InterruptCurrentRun()` calls `Stop()`), but
*stopping never cancels* (`Stop()` never touches the token). `Stop()` means
"the workflow reached its own normal end" (e.g. the `/exit` command via
`ExitCliCommandHandler`, which is the only place outside `CliWorkflow`
itself that ever calls it); `InterruptCurrentRun()` means "abort early,"
and abort-early necessarily implies stopped, so it also flips `Status`. The
reverse would be wrong: if `Stop()` also cancelled the token,
`ExitCliCommandHandler` calling `Stop()` on itself to end normally would
make its own in-flight `CancellationToken` read as cancelled before it
finished — indistinguishable from an actual interruption, for any handler
that does further token-sensitive work after calling `Stop()`. Keeping the
implication one-directional gets both properties: `Status` alone is always
sufficient to know "is this over," while `CancellationToken
.IsCancellationRequested` still separately answers "was it aborted, or did
it end on its own" for anything that cares — a normal `/exit` never sets
it, only `InterruptCurrentRun()` does.

The DI-scope-per-run backstop sweep added for #71 stays regardless — it's a
correctness net for scopes that never observe cancellation at all (a
handler that doesn't check its token), not something this decision removes.

## Alternatives considered

- **Keep `Environment.Exit`, just move it after an awaited drain** — still
  requires a cross-thread signal into the main loop to know when it's safe
  to call `Exit`, which is exactly the cooperative-cancellation mechanism
  this ADR adopts; doing that and then still hard-killing the process gains
  nothing over letting `Run` return normally.
- **`CliApp` owns the `CancellationTokenSource`, threaded through
  `ICliWorkflowRun.RespondToAsk(ask, cancellationToken)`/`MoveToNext
  (cancellationToken)` as a parameter** — the first shape this ADR shipped
  with. Rejected in favor of `CliWorkflow` owning it: the token is a
  per-run-lifetime resource exactly like the `IServiceScope` `CliWorkflow`
  already constructor-injects into each `CliWorkflowRun`, so treating it as
  a call parameter instead was inconsistent with that existing pattern and
  forced a public signature change on `ICliWorkflowRun` for no benefit —
  `CliWorkflow` was already the layer sitting between `CliApp` and each
  run's construction.
- **Cancel only between commands, not mid-command** — simpler (no token
  reaching `_sender.Send`), but leaves the exact race #74 reported: a
  long-running command's `finally`/`Dispose()` still can't be interrupted,
  so Ctrl+C during a slow command would hang until that command finishes on
  its own.
- **Synchronously block `Console.ReadLine()` and accept the abandoned-thread
  cost** — this is effectively what was chosen (`AskAsync` abandons the
  pending read on cancellation); called out here because the alternative —
  actually killing that read, e.g. via `Console.In.Close()` — was rejected
  as needlessly fragile (`Console.In` is process-global state) for a
  background thread the CLR already reclaims on normal process exit.

## Consequences

- `ICliIo.Ask()` → `AskAsync(CancellationToken)` is a breaking change to a
  public interface — every implementation needed to change shape to
  support the race against `Console.ReadLine()`.
- `ICliWorkflow` gains `CancellationToken` and `InterruptCurrentRun()` —
  also a breaking addition, but one `ICliWorkflowRun` avoids entirely:
  `RespondToAsk`/`MoveToNext` keep their exact original signatures.
- A command handler that ignores its `CancellationToken` still runs to
  completion on Ctrl+C — cooperative cancellation only helps handlers that
  actually check it. This was already implicitly true of MediatR's
  `IRequest`/`CancellationToken` contract; this ADR doesn't add a new
  obligation, just makes KitCli's own host loop honor it end to end instead
  of short-circuiting past it with `Environment.Exit`.
- Ctrl+C during `Console.ReadLine()` now returns control to the app (and
  the process exits normally, shortly after) rather than an instant kill —
  a small, expected latency change, not a regression.
- A `CliWorkflowRun` captures its `CancellationToken` at construction time,
  from whatever `CliWorkflow.CancellationToken` returns at that moment.
  This is safe even though cancellation might not fire until later:
  `CancellationToken` is a lightweight handle back to its source, not a
  snapshot of `IsCancellationRequested` — a token captured before
  `InterruptCurrentRun()` is called still observes that later call.
- Because `InterruptCurrentRun()` calls `Stop()`, `Status == Stopped` no
  longer tells you *why* a workflow ended — only `CancellationToken
  .IsCancellationRequested` still distinguishes "aborted" from "reached its
  own end." Nothing in KitCli surfaces that distinction to a consuming app
  today (`OnSessionEnd` only receives `Workflow.Runs`), but the bit is
  there, checkable via `Workflow.CancellationToken`, if a future hook wants
  it.
