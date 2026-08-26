# 0005. How much of the run loop can the two hosts share?

- **Status:** In Review
- **Superseded in part by** [ADR 0013](../adr/0013-merge-the-hosts-and-name-the-variant-headless.md):
  the shape recommended below — the loop hoisted onto `CliApp` behind an
  ask-source seam — was not built. `TerminalCliApp` held nothing `CliApp`
  did not, so it folded in and the seam disappeared with it. Every finding
  about *what* is wrong stands; the design for fixing it did not survive
  writing. The two hosts were called `TerminalCliApp` and `ArgsCliApp` at
  the time; they are now `CliApp` and `HeadlessCliApp`.
- **Spike:** [#169](https://github.com/KitCli/KitCli/issues/169)
- **Time-box:** none agreed — filed alongside this investigation
- **Date:** 2026-08-25, revised 2026-08-26

## Verdict

New complexity. [#167](https://github.com/KitCli/KitCli/issues/167) reads as
a missing `MoveToNext()` call, and it is one. But the loop that call belongs
to already exists twice, once in each host, and the second copy is where the
step gets dropped. Fixing #167 by adding a third thing to `ArgsCliApp` keeps
the copy that caused it.

Most of this is genuinely small. `TerminalCliApp.Run` and `ArgsCliApp.Run`
run the same eight steps around the same `Workflow.NextRun()`, and differ in
two places only: where an ask comes from, and when the session stops. Leave
the loop in one place and a one-shot invocation drives its chain, because
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

#167 stays open as the parent. #169 closes — it answered its question.

1. **#168 first, and it is a hard blocker.** Decide what a run that ends on a
   queued step does: reach a named terminal state, or return a diagnostic
   outcome. Today `Workflow.Stop()` hides the question in one-shot mode.
   Nothing below is safe until an unterminated chain has a defined ending.
2. **An ADR**, amending [ADR 0005](../adr/0005-args-driven-cli-app.md), which
   is still `Proposed`.
3. **The host change itself**, closing #167 rather than a follow-up ticket
   doing so: the truncation *is* the second copy.
4. **Cover the host.** A `KitCli.Playground.Scenarios` chain run under both
   apps. The loop has one test today, which is how #167 shipped — see
   [#26](https://github.com/KitCli/KitCli/issues/26) and the *Add Tests for
   the App Host and Scenarios* milestone.
5. **Docs, in the same PR as the change they describe** —
   [`0002-cli-app-host.md`](../concepts/0002-cli-app-host.md), user guides
   0002, 0003 and 0007, and `CHANGELOG.md`.

## What was built, and where it departed

[#172](https://github.com/KitCli/KitCli/pull/172) shipped 2 through 5 as one
change, recorded in
[ADR 0013](../adr/0013-merge-the-hosts-and-name-the-variant-headless.md). Two
departures worth the paper trail.

**The seam was the wrong answer, twice over.** This document proposed hoisting
the loop behind a method supplying the next ask. Written out, that method had
one real implementation — `Io.AskAsync` — and `CliApp` already holds `ICliIo`.
The seam abstracted something the base class had all along, and forced the
one-shot host to implement a member meaning "not me". Three variants were tried
and dropped: the ask-source seam, a base-class default returning none, and an
`ICliIo` that yields the args once. What shipped instead: `TerminalCliApp`
folded into `CliApp`, `ArgsCliApp` became `HeadlessCliApp` and overrides `Run`.
No seam exists.

**#168 did not go first.** The order above was not followed — the host change
shipped ahead of it, so a chain that hands on forever now leaves a headless
invocation running rather than truncating silently. That trade is recorded in
ADR 0013's consequences and in the chaining guide, and
[`/test-unending-chain`](https://github.com/KitCli/KitCli/blob/main/KitCli.Playground.Scenarios/TestUnendingChainCliCommand.cs)
demonstrates it. The blocker call stands: this is a worse failure than the one
fixed, and it is live.

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

- What should a headless invocation exit with when the ask resolves to no
  command? Still 0, so a script cannot tell success from failure. A handler
  that throws now exits non-zero, which is the only part of this answered.
- Three of these have since been answered, and are recorded where they
  belong rather than here: a run parked at a reusable checkpoint with
  nothing left to ask ends the session unfinished
  ([#174](https://github.com/KitCli/KitCli/issues/174)); the shipped loop
  reads the run's current status rather than `WasChangedTo`'s history, which
  a `while` requires; and a chain that hands on forever really does never
  return, reproduced by `/test-unending-chain`
  ([#173](https://github.com/KitCli/KitCli/issues/173)).
- Does anything outside this repo name the old types? v3.0.0 shipped the
  rename, so the answer arrives as consumer reports rather than analysis.
  `KitCli.Tooling.Release` is the one known case, pinned to an older package
  and still on `ArgsCliApp` until its pin moves.

## Out of scope

- Which queued command a chain picks, and how it is built —
  [#124](https://github.com/KitCli/KitCli/issues/124) and
  [#147](https://github.com/KitCli/KitCli/issues/147). This is about who
  drives the chain, not what it selects.
- Letting one class run in both modes. ADR 0005's rejection of a runtime
  `args.Length` branch stands.
- `CliAppBuilder`'s configuration and registry surface.
- Instruction parsing and validation.
