# 0005. How much of the run loop can ArgsCliApp and TerminalCliApp share?

- **Status:** In Review
- **Spike:** [#169](https://github.com/KitCli/KitCli/issues/169)
- **Time-box:** none agreed — filed alongside this investigation
- **Date:** 2026-08-25

## Verdict

New complexity. [#167](https://github.com/KitCli/KitCli/issues/167) reads as
a missing `MoveToNext()` call, and it is one. But the loop that call belongs
to already exists twice, once in each host, and the second copy is where the
step gets dropped. Fixing #167 by adding a third thing to `ArgsCliApp` keeps
the copy that caused it.

Most of this is genuinely small. `TerminalCliApp.Run` and `ArgsCliApp.Run`
run the same eight steps around the same `Workflow.NextRun()`, and differ in
two places only: where an ask comes from, and when the session stops. Move
the loop onto `CliApp` and a one-shot invocation drives its chain because
there is no longer a second loop that doesn't.

The complexity is what that exposes. Once a one-shot invocation ends when its
*run* ends rather than after its first command, a chain that never ends
([#168](https://github.com/KitCli/KitCli/issues/168)) leaves the process
waiting instead of exiting early — a worse failure than the one being fixed,
and reason enough that #168 is a blocker rather than a neighbour. Around
that: `Run`'s signature moves, which breaks any consumer not going through
`CliAppBuilder`; [ADR 0005](../adr/0005-args-driven-cli-app.md) chose this
split deliberately and has to be amended, not quietly contradicted;
`OnMovingPastAsk` starts firing in one-shot mode, which a concept doc
currently promises it never does; and the loop being rewritten has one test.

## Recommendation

#167 stays open as the parent. #169 closes — it answered its question. The
build hangs off #167 in this order.

1. **#168 first, and it is a hard blocker.** Decide what a run that ends on a
   queued step does: reach a named terminal state, or return a diagnostic
   outcome. Today `Workflow.Stop()` hides the question in one-shot mode.
   Nothing below is safe until an unterminated chain has a defined ending.
2. **ADR — the run loop belongs to `CliApp`, and a host is an ask source.**
   Amends ADR 0005, which is still `Proposed`. It keeps that ADR's actual
   decision, that the mode is a compile-time subclass choice, and drops only
   the part where each subclass owns a whole `Run`. It is not the
   `args.Length` branch ADR 0005 rejected: no runtime `if` decides the mode.
3. **`refactor(host)!` — hoist the loop and `ExecuteRunOperation` onto
   `CliApp`**, behind two seams: one supplying the next ask, one deciding
   whether the session continues. `TerminalCliApp` supplies `Io.AskAsync`;
   `ArgsCliApp` supplies the joined args and ends the session once its run
   reaches `Finished`. This closes #167 rather than a follow-up ticket
   doing so: the truncation *is* the second copy.
4. **Cover the host.** A `KitCli.Playground.Scenarios` chain run under both
   apps, asserting the args app prints every step and its run reaches
   `Finished`. The loop has one test today, which is how #167 shipped —
   see [#26](https://github.com/KitCli/KitCli/issues/26) and the
   *Add Tests for the App Host and Scenarios* milestone.
5. **Docs, in the same PR as the change they describe.**
   [`0002-cli-app-host.md`](../concepts/0002-cli-app-host.md) (the "runs
   exactly one command" and hook-firing paragraphs),
   [user guide 0002](../user-guides/0002-creating-an-args-app.md) and
   [0007's gotchas](../user-guides/0007-chaining-commands.md), and
   `CHANGELOG.md`.

## What was established

- **The two hosts differ in two behaviours.** Both call `OnSessionStart`,
  `Io.Pause`, `SetUpEventHandlers`, `Workflow.NextRun`, `OnRunCreated`,
  `WriteOutcomes`, `OnRunComplete` and `OnSessionEnd` identically. Only the
  ask source and the stop rule differ.
- **`ArgsCliApp` stops on a command, not on a run.** `Workflow.Stop()` fires
  after one `RespondToAsk` whatever state the run reached, so a run parked at
  `MovePastAsk` is abandoned there — never `Finished`, so its DI scope is
  never disposed.
- **The continuation branch needs nothing one-shot-specific.**
  `TerminalCliApp.ExecuteRunOperation` already reads a queued `MovePastAsk`
  and calls `run.MoveToNext()`. Sharing it is a move, not a rewrite.
- **The stop rule is the real design question.** A one-shot session has to
  end when its run is finished, and the check has to sit outside
  `Workflow.NextRun()` so an exhausted ask source cannot create a spare run
  and respond to it with `null`. Putting it in a lifecycle hook works but
  spends `OnRunComplete`, which consumers override; a separate seam keeps all
  six hooks theirs.
- **Stopping the workflow is the sanctioned move.** `ExitCliCommandHandler`
  ends an interactive session with `Workflow.Stop()`, and ADR 0005 rejected
  reaching into `run.State` for the same purpose. A host may stop a workflow;
  it may not finish a run.
- **Only one host handles an exceptional run.** `RethrowIfExceptional` is
  `TerminalCliApp`'s, added so an unaccounted-for failure ends the session
  ([#104](https://github.com/KitCli/KitCli/issues/104)). A one-shot
  invocation writes the `ExceptionOutcome` and exits 0. Sharing the loop
  makes this one behaviour by default, which is a change worth deciding
  rather than inheriting.
- **The public surface moves.** `ArgsCliApp.Run(List<IOutcomeIoWriter>,
  string[])` and `TerminalCliApp.Run(List<IOutcomeIoWriter>, string[]?)` are
  both public, and `CliAppBuilder.Run` dispatches on the concrete type.
  Collapsing them to one `Run` is a major bump under
  [ADR 0012](../adr/0012-derive-version-bumps-from-the-public-api-diff.md).
  `CliAppBuilder`'s no-args guard survives unchanged.

## Evidence

Observed on a clean checkout of `a41527f`, packages at 2.0.0:

```bash
dotnet run --project KitCli.Playground.App.Args -- /test-next
```

`/test-next` is `TestNextCliCommand` in `KitCli.Playground.Scenarios`, five
handlers chained with `ByMovingToCommand` and ended with `ByFinallySaying`.
The one-shot app prints step 0 and stops:

```
Initial Command Ran (0)
Run state changes: Running, MovePastAsk
Run outcomes achieved: RanCliCommandOutcome, SayOutcome, ProvidedNextCliCommandOutcome
```

Exit code 0. The remaining facts come from reading `KitCli/CliApp.cs`,
`KitCli/ArgsCliApp.cs`, `KitCli/TerminalCliApp.cs`, `KitCli/CliAppBuilder.cs`
and `KitCli.Workflow/CliWorkflow.cs`.

## Open questions

- What should a one-shot invocation exit with when its run ends
  `Exceptional`, or when the ask never resolved to a command? Both exit 0
  today, so a script cannot tell success from failure.
- What should happen when a chained step in one-shot mode stops at a reusable
  checkpoint and wants another ask, with no more args to give it?
- `ExecuteRunOperation` branches on `WasChangedTo(MovePastAsk)`, and
  `CliWorkflowRunState.WasChangedTo` tests the run's history rather than its
  current status. What that means for a run that moves past an ask more than
  once is read from source and not reproduced — #168 carries it.
- Does anything outside this repo call `ArgsCliApp.Run` or
  `TerminalCliApp.Run` directly rather than through `CliAppBuilder.Run`?

## Out of scope

- Which queued command a chain picks, and how it is built —
  [#124](https://github.com/KitCli/KitCli/issues/124) and
  [#147](https://github.com/KitCli/KitCli/issues/147). This is about who
  drives the chain, not what it selects.
- Letting one class run in both modes. ADR 0005's rejection of a runtime
  `args.Length` branch stands.
- `CliAppBuilder`'s configuration and registry surface.
- Instruction parsing and validation.
