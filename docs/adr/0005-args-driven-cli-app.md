# 0005. Split CliApp into ArgsCliApp and TerminalCliApp

Status: Proposed
Date: 2026-08-22

## Context

KitCli apps could only ever be driven interactively: `CliApp.Run` was a
`while` loop that called `Io.Ask()` → `Console.ReadLine()` on every
iteration. There was no way to invoke a command in one shot from process
args (`myapp /command --flag value`) — `Program.cs` never read `args` at
all, and nothing in `CliAppBuilder`/`CliApp` had a seam for it. See
[0002-cli-app-host.md](../concepts/0002-cli-app-host.md) for how the interactive loop
works today.

## Decision

Split `CliApp`'s execution model into two subclasses:

- `TerminalCliApp` — the pre-existing interactive loop, unchanged in
  mechanics, just moved off the base class.
- `ArgsCliApp` — new. Joins `args` into a single ask string, feeds it
  through the same `RespondToAsk` pipeline an interactive ask would use,
  then calls `Workflow.Stop()` once that one run completes.

`CliApp` itself keeps only what both share: the `Workflow`/`Io` references,
`SetUpEventHandlers`, `WriteOutcomes`, and the six lifecycle hooks. An app
picks its mode once, by which subclass it extends — `TestCliApp : ArgsCliApp`
or `TestCliApp : TerminalCliApp` — not by a flag checked at launch.
`CliAppBuilder.Run(string[]? args)` resolves the concrete `CliApp` from DI
and dispatches to whichever subclass it turns out to be, throwing a specific
error if it's an `ArgsCliApp` with no args rather than doing nothing.

Two smaller, related changes ride along:

- `BasicCliApp`/`WithBasicCli()` renamed to `BasicTerminalCliApp`/
  `WithBasicTerminalApp()`, and `WithCli<TCliApp>()` renamed to
  `WithApp<TCliApp>()` — the old names no longer say which mode "basic"
  means. **This is a breaking rename** for any existing consumer.
- `KitCli.Playground` (one project, wired only for the interactive demo)
  split into `KitCli.Playground.App.Terminal` (the pre-existing paging
  demo, moved as-is) and `KitCli.Playground.App.Args` (a new one-shot demo,
  including a minimal `EchoCliCommand` that prints back whatever args it's
  given), alongside the pre-existing `KitCli.Playground.Scenarios`.

## Alternatives considered

**A single `CliApp.Run(args)` branching internally on `args.Length`.**
Simplest to write, but it hides which mode an app runs in behind a runtime
`if` inside a shared method — an app author can't tell from the class
declaration alone whether "run with no args" is supported or is a bug.
Also means `CliApp` permanently owns two divergent execution strategies
under one method, growing harder to reason about as either grows.

**A separate `ICliAppRunner` strategy, composed with `CliApp` and chosen by
`CliAppBuilder`.** Decouples "how this launch runs" from "what this app is"
without forcing a subclass choice — genuinely more flexible, since the same
app class could run either way depending on the caller. Rejected as
over-engineered for two ~20-line execution shapes; introduces an interface
and two implementations for a distinction that, in practice, an app commits
to for its whole lifetime anyway.

**Mutating `run.State` directly from `CliApp` to force `ReachedFinalOutcome`
after one command.** Would have made `ArgsCliApp` a few lines shorter.
Rejected outright — only `CliWorkflowRun` is allowed to change its own
state; see [0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md).
`Workflow.Stop()` was used instead — the same public method
`ExitCliCommandHandler` already calls.

**Exposing `ICliWorkflow.CreateNewRun()` as a second, public entry point for
creating runs**, called directly from `ArgsCliApp` instead of going through
`NextRun()`. Tried during implementation, then reverted: it bypassed
`NextRun()`'s reuse-or-create logic and the single-active-run invariant
documented in 0010-workflow-run-state-machine.md ("the only caller of `NextRun()`
is `CliApp.Run`'s own loop"). `ArgsCliApp` goes through `Workflow.NextRun()`
like `TerminalCliApp` does.

## Consequences

- An app's run mode is now a compile-time fact, not a launch-time one. This
  is a real limitation: an app that genuinely wants to support both an
  interactive session *and* a one-shot invocation needs two classes (or a
  future change to this decision), not one.
- `ArgsCliApp` currently supports exactly one command per invocation —
  `Workflow.Stop()` fires unconditionally after the single `RespondToAsk`
  call, regardless of what state the run reached. A command that would
  normally need a `MoveToNext()` continuation (multi-step, paging) isn't
  driven past its first step in one-shot mode.
- `BasicCliApp`/`WithBasicCli()`/`WithCli<T>()` renames are breaking for any
  existing consumer of those exact names — flagged via `!` in the commit
  type and the CHANGELOG entry, per this repo's versioning policy in
  CONTRIBUTING.md (no version bump in this PR; that happens separately at
  release time).
- Surfaced, not fixed here: `AddCli<TCliApp>()` hardcodes
  `Assembly.GetEntryAssembly()` as a required source of at least one
  `CliCommand`, which doesn't fit a thin-host-plus-separate-command-library
  layout cleanly — `KitCli.Playground.App.Args` needed a throwaway command
  (`EchoCliCommand`) in its own assembly to satisfy that scan. Tracked as
  follow-up work, not addressed in this change.
