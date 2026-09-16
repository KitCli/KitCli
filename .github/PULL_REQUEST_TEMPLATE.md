<!--
Title must follow Conventional Commits: <type>(scope): <description>
  types: feat | fix | docs | chore | refactor | test | ci
  scope (optional): abstractions | instructions | commands | workflow | host | tooling
  breaking change: add "!" right before the colon, e.g. refactor(host)!: ...
  example: feat(commands): give a command extra names
  example (breaking): refactor(host)!: rename RespondToNext to MoveToNext
Description is lowercase, imperative mood, no trailing period, no "fix stuff."
This becomes the squash-merge commit title, i.e. the CHANGELOG line.

Keep the whole description under 30 lines. Explain the problem before the
fix, in plain language a reader with no context on this repo can follow.
Reach for a code snippet over a paragraph wherever one shows the change
faster.

Write "Linked issue: #N", never "Fixes #N" / "Closes #N" — those auto-close
the issue on merge, and that needs agreeing first, separately.
-->

## What

## Why

Linked issue: #

Tested: unit tests, a `KitCli.Playground.Scenarios` scenario, or manual — say which.

## Kind of change

- [ ] Bug fix
- [ ] Feature
- [ ] Refactor
- [ ] Breaking change
- [ ] Tech debt
- [ ] Docs / process

Breaking change or new cross-cutting pattern: add an ADR in `docs/adr/`.
Behaviour change: update `CHANGELOG.md`, and any concept doc it affects.
