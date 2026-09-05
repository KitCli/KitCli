# 0017. Let a command ask its I/O whether anyone can answer

Status: Proposed
Date: 2026-09-05

## Context

A command had no way to learn whether the app it runs in is headless. One
that would confirm, page, or prompt in an interactive session has nobody
to ask under `myapp /command --flag value`, and nothing told it so.

The wider practice — gh's `CanPrompt()`, Spectre.Console's `Interactive`
capability, Symfony's `isInteractive()` — phrases it as "can I prompt?"
and hangs it on the I/O object every command already has, rather than on
the app.

## Decision

`ICliIo` gains `CanAsk`, with a default body of `true` so no existing I/O
breaks. `HeadlessCliIo` derives from `CliIo`, answers `false`, and throws
`NoInputException` if asked anyway — a programming error, not an end of
input. `CliAppBuilder.Run` registers the I/O from the app it was given,
unless a registry registered an `ICliIo` first, so a consumer's own I/O
still wins. `AddCliAbstractions()`, which only ever registered `CliIo`,
goes.

## Alternatives considered

- **The app answers `IsHeadless`** under a one-member interface — no
  wiring, but an interface invented to carry a boolean, and not where the
  ecosystem puts the question.
- **A `canAsk` flag threaded through registration** into `CliIo`'s
  constructor — a flag passed down three layers to set one property.
- **Register `CliIo`, then `Replace` it** for a headless app — registers
  the wrong one first.
- **A second builder door, `WithHeadlessApp<T>()`**, each door registering
  its I/O — more public API than the question needs.
- **Split `CliApp` into interactive and headless siblings** so the doors
  constrain at compile time — every custom interactive app would extend a
  new name. Too dramatic for the gain.
- **`CloseInput()` on the I/O**, called by the headless app at session
  start — a singleton that flips state once, and a custom I/O could ignore
  it.
- **Detect a terminal on stdin** — answers "is a person at the keyboard?",
  which disagrees with a headless run launched by hand.

## Consequences

- Breaking for a consumer that called `AddCliAbstractions()` directly;
  additive for everyone else.
- The builder reads the registered app's type to pick the I/O. That is
  the one runtime choice left, and it sits in the composition root beside
  the existing "headless app with no args" check.
- Asking a headless I/O throws, so a command must check `CanAsk` first.
