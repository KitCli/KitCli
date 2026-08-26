# 0005. Split CliApp into ArgsCliApp and TerminalCliApp

Status: Proposed
Date: 2026-08-22

Reversed by [0013](0013-merge-the-hosts-and-name-the-variant-headless.md).
**The type names below no longer exist in the codebase.** `TerminalCliApp`
folded back into `CliApp`, and `ArgsCliApp` became `HeadlessCliApp`. Read
this for the reasoning that got KitCli a non-interactive host at all; read
0013 for the shape that shipped.

## Context

KitCli apps could only be driven interactively: `CliApp.Run` was a `while`
loop calling `Console.ReadLine` every iteration. There was no way to invoke
a command in one shot from process args — `Program.cs` never read `args`,
and nothing had a seam for it.

## Decision

Split `CliApp`'s execution model into two subclasses.

- `TerminalCliApp` — the pre-existing interactive loop, moved off the base
  class unchanged.
- `ArgsCliApp` — new. Joins `args` into one ask, feeds it through the same
  pipeline an interactive ask uses, then calls `Workflow.Stop()`.

`CliApp` keeps only what both share: the `Workflow`/`Io` references,
`SetUpEventHandlers`, `WriteOutcomes`, and the six lifecycle hooks. An app
picks its mode by which subclass it extends, not by a flag checked at
launch. `CliAppBuilder.Run(string[]? args)` resolves the concrete app and
dispatches, throwing a specific error for an `ArgsCliApp` with no args.

Two renames ride along, **breaking for existing consumers**:
`BasicCliApp`/`WithBasicCli()` become
`BasicTerminalCliApp`/`WithBasicTerminalApp()`, and `WithCli<TCliApp>()`
becomes `WithApp<TCliApp>()`. The playground splits to match.

## Alternatives considered

- **One `Run(args)` branching on `args.Length`** — hides the mode behind a
  runtime `if`, so the class declaration no longer says whether "run with
  no args" is supported or a bug.
- **An `ICliAppRunner` strategy chosen by `CliAppBuilder`** — genuinely
  more flexible, and over-engineered for two ~20-line execution shapes an
  app commits to for its whole lifetime.
- **Mutating `run.State` from `CliApp` to force a final outcome** — only
  `CliWorkflowRun` may change its own state. `Workflow.Stop()` was used
  instead, the same method `ExitCliCommandHandler` already calls.
- **Making `ICliWorkflow.CreateNewRun()` public and calling it directly** —
  tried, then reverted: it bypasses `NextRun()`'s reuse logic and the
  single-active-run invariant.

## Consequences

- An app's mode is a compile-time fact. An app wanting both an interactive
  session *and* a one-shot invocation needs two classes.
- `ArgsCliApp` runs exactly one command per invocation: `Workflow.Stop()`
  fires unconditionally after the single ask, so a chained step is never
  driven. **This is the defect 0013 exists to fix**
  ([#167](https://github.com/KitCli/KitCli/issues/167)).
- The renames break any consumer of the old names.
- Surfaced, not fixed: `AddCli<TCliApp>()` hardcodes
  `Assembly.GetEntryAssembly()` as a required source of at least one
  `CliCommand`, which does not fit a thin host plus a separate command
  library. The args playground needed a throwaway command to satisfy it.
