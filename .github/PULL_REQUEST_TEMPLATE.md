<!--
Title must follow Conventional Commits: <type>(scope): <description>
  types: feat | fix | docs | chore | refactor | test | ci
  scope (optional): abstractions | instructions | commands | workflow | host | tooling
  breaking change: add "!" right before the colon, e.g. refactor(host)!: ...
  example: feat(instructions): add quoting support to the tokenizer
  example (breaking): refactor(host)!: rename RespondToNext to MoveToNext
Description is lowercase, imperative mood, no trailing period, no "fix stuff."
This becomes the squash-merge commit title, i.e. the CHANGELOG line.
-->

## What

## Why

Linked issue: #

## How

## Tested

- [ ] Unit tests
- [ ] `KitCli.Playground.Scenarios` scenario
- [ ] Manual

## Kind of change

- [ ] Bug fix
- [ ] Feature
- [ ] Refactor
- [ ] Breaking change
- [ ] Tech debt
- [ ] Docs / process

If **Breaking change** or a new cross-cutting pattern/project boundary
change: needs an ADR in `docs/adr/` in this PR. If this changes behavior:
update `CHANGELOG.md` under `[Unreleased]`.
