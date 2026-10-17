# 0006. Drive Ctrl+C shutdown with a CancellationToken, not Environment.Exit

Status: Proposed
Date: 2026-08-22

Written before [ADR 0013](0013-merge-the-hosts-and-name-the-variant-headless.md)
renamed the hosts. Where this says `TerminalCliApp`, read `CliApp`; where it
says `ArgsCliApp`, read `HeadlessCliApp`.

## Context

`CliApp.SetUpEventHandlers` wired `Io.OnCancel` to `Console.CancelKeyPress`,
which .NET fires on its own thread pool thread — concurrently with whatever
the main loop was doing mid-command:

```csharp
Io.OnCancel(() =>
{
    _workflow.Stop();
    OnSessionEnd(_workflow.Runs);
    Environment.Exit(exitCode: 0);
});
```

Two problems followed, reported in #74. `Stop()` ran on the cancel thread
while the main thread could still be using services from the in-flight
run's DI scope — a cross-thread dispose race. And `Environment.Exit`
terminates immediately, skipping any `Dispose()` that has not already run:
flushing writes, committing a transaction, releasing a lock. That is a
correctness problem, not a leak.

## Decision

Replace the abrupt exit with cooperative cancellation, owned by
`ICliWorkflow` rather than `CliApp`.

- `CliWorkflow` owns a `CancellationTokenSource`, exposing
  `CancellationToken` and `InterruptCurrentRun()`. It hands the token to
  each run's constructor, exactly as it already hands over the
  `IServiceScope` ([ADR 0002](0002-di-scope-per-workflow-run.md)) — not
  threaded through `RespondToAsk`/`MoveToNext` on every call.
- `CliApp.SetUpEventHandlers` wires `Io.OnCancel` to
  `Workflow.InterruptCurrentRun`, and nothing else.
- `CliIo`'s handler sets `e.Cancel = true`, suppressing .NET's own abrupt
  termination so the app drives shutdown.
- `CliWorkflowRun` passes the token to `_sender.Send`, so an in-flight run
  unwinds through its normal `catch`/`finally` instead of being disposed
  out from under it.
- `ICliIo.Ask()` becomes `AskAsync(CancellationToken)`, which runs
  `Console.ReadLine` on a background thread and awaits it via
  `.WaitAsync(cancellationToken)`, returning `null` if cancellation wins.
  `WaitAsync` rather than the older `WhenAny(read, Delay(Infinite, token))`
  idiom, because that one leaks a token registration per ask —
  [dotnet/runtime#46603](https://github.com/dotnet/runtime/issues/46603).

`InterruptCurrentRun()` calls `Stop()`; `Stop()` never touches the token.
**The implication is deliberately one-directional.** Cancelling always
stops, so `Status` alone always answers "is this over". Stopping never
cancels, so a handler calling `Stop()` to end normally does not see its own
token read as cancelled. `IsCancellationRequested` separately answers "was
it aborted, or did it end on its own".

## Alternatives considered

- **Keep `Environment.Exit`, after an awaited drain** — needs the same
  cross-thread signal this ADR adopts, then still hard-kills the process.
- **`CliApp` owns the token, threaded through `ICliWorkflowRun` as a
  parameter** — the first shape this shipped with. Rejected: the token is a
  per-run resource like the scope, so a call parameter was inconsistent and
  forced a public signature change for no benefit.
- **Cancel only between commands** — simpler, and leaves the exact race #74
  reported: Ctrl+C during a slow command hangs until it finishes.
- **Actually killing the blocked read, via `Console.In.Close()`** —
  `Console.In` is process-global state; abandoning a background thread the
  CLR reclaims anyway is less fragile.

## Consequences

- `ICliIo.Ask()` → `AskAsync(CancellationToken)` breaks every
  implementation of a public interface.
- `ICliWorkflow` gains two members — also breaking — but `ICliWorkflowRun`
  keeps its exact signatures.
- **A handler that ignores its token still runs to completion on Ctrl+C.**
  Cooperative cancellation only helps handlers that check.
- Ctrl+C during a read returns control to the app and exits shortly after,
  rather than instantly. A small, expected latency change.
- `Status == Stopped` no longer says *why* a workflow ended. The bit is
  there on `CancellationToken`, but nothing surfaces it to a consuming app
  today.
