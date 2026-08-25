# 0013. Merge the hosts and name the variant headless

Status: Proposed
Date: 2026-08-26

## Context

[#167](https://github.com/KitCli/KitCli/issues/167): a one-shot invocation
ran the first command of a chain and exited 0, dropping the rest in
silence. [ADR 0005](0005-args-driven-cli-app.md) had split `CliApp` into
`TerminalCliApp` and `ArgsCliApp`, which copied the host loop into two
places; the args copy is the one that never drove a queued step.

The split also never paid for itself. `CliApp` already holds `ICliIo` — it
needs it for `Pause`, `Say` and `OnCancel` — so `TerminalCliApp` held
nothing the base did not. Every attempt to abstract "where an ask comes
from" pushed a seam into the base that only one subclass ever meant.

## Decision

`TerminalCliApp` folds into `CliApp`, which *is* the interactive app.
`ArgsCliApp` becomes `HeadlessCliApp` and overrides `Run` to answer one ask
with nobody to prompt. `ExecuteRunOperation` keeps its name, takes the ask,
and drives the run past every step it queues — the piece both hosts need,
now written once. `BasicTerminalCliApp` and `WithBasicTerminalApp()` become
`BasicCliApp` and `WithBasicApp()`; the playground apps become
`KitCli.Playground.App` and `KitCli.Playground.App.Headless`.

**Headless, not one-shot.** A headless invocation is not limited to one
command — a chain runs every step. It is limited to one *run*, because
nothing is attached to its input, so no second ask can start another.
"One-shot" names a symptom that is not even true, and is the misreading
behind #167. Prose may still say "a one-shot invocation".

## Alternatives considered

- **An ask-source seam on `CliApp`** (`SourceAsk`, defaulting to none).
  Made the base know about sourcing asks, and made `HeadlessCliApp`
  implement a member meaning "not me" — a refused bequest.
- **An args-backed `ICliIo`** yielding the args once, then end-of-input.
  Honest, but it moves the mode into DI registration and leaves both app
  classes empty.
- **Copying `TerminalCliApp`'s continuation into `ArgsCliApp`.** Fixes #167
  and keeps the duplicate that caused it.
- **Merging both hosts into one class.** Loses the type test
  `CliAppBuilder` uses to reject a headless launch with no args, turning a
  named error into a process waiting on input that never comes.

## Consequences

- Reverses ADR 0005, which is still `Proposed`. Breaking for every consumer
  naming the old types.
- `KitCli.Tooling.Release` consumes the published `KitCli` package, not the
  project, so it stays on `ArgsCliApp` until a release ships the new name
  and its pin is bumped. The rename is a two-step.
- The continuation loop reads the run's *current* status rather than
  `WasChangedTo`'s history, which a `while` requires — history answers true
  forever once a run has moved past an ask.
- A headless app now rethrows an `ExceptionOutcome` like an interactive one,
  so a throwing handler ends it with a non-zero exit instead of 0.
- A chain that never ends now leaves a headless invocation running rather
  than truncating it silently. That is
  [#168](https://github.com/KitCli/KitCli/issues/168), and it is why #168
  blocks the rest of this work.
