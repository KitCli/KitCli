---
name: deliver-issue
description: Take a delivery-stage GitHub issue in this repo from branch to open PR — implementing it, running the tests, firing the ADR/CHANGELOG/concept-doc triggers it earns, and opening a PR that mirrors the issue's metadata. Use when asked to work an issue, implement a fix or feature, or open a PR for work already written.
---

# Delivering an issue

`CONTRIBUTING.md` holds the standing rules — the label axes, the commit
format, the doc kinds. This skill holds the *order* they apply in, so
none of them is remembered only after the PR is open.

The review → issues → docs loop is a different skill:
`repo-operating-model`. This one starts once an issue exists.

## 1. Read the issue before branching

Confirm it is delivery-stage: scoped, and titled like the commit that
finishes it (`feat(workflow): ...`). An idea-stage title ("No way to X")
means the work is not carved out yet — say so rather than guessing a
scope.

Note its labels and milestone now. Step 5 has to reproduce them, and
GitHub will not copy them for you.

**Docs-only work skips this skill.** It goes straight to `main`.

## 2. Branch off `main`

Short-lived, one logical change. If the change needs "and" to describe,
it is two branches and two PRs. Hard ceiling of 20 files, 10-15
preferred — find the split before writing code, not when the diff is
already too big.

## 3. Implement, matching what is there

- Match the file's existing patterns. A rename, an extracted helper or a
  new abstraction nobody asked for is a separate PR.
- Test doubles go in the test project's `TestHelpers/` the first time
  they are written, named `Test*`.
- Build fixtures from real `Command`, `Outcome` and `Artefact` types, so
  a fixture cannot drift from the shape it stands in for.

## 4. Run the build and the docs triggers

```
dotnet restore KitCli.sln
dotnet build KitCli.sln
dotnet test KitCli.sln
```

All six test projects must pass; branch protection enforces it anyway,
so failing here is cheaper.

Then apply whichever triggers the change earned, **in this PR**:

| The change | What it also needs |
|---|---|
| Breaking, cross-cutting, or a project/package boundary move | an ADR in `docs/adr/` |
| Any behaviour a consumer can observe | a `CHANGELOG.md` line under `[Unreleased]` |
| Behaviour a concept doc describes | that doc in `docs/concepts/` updated |

If the change touches `docs/`, preview with `docfx docfx.json --serve`.
The Docs workflow builds with `--warningsAsErrors`, so a link docfx
cannot resolve fails CI — links to a folder, and links outside `docs/`,
are the two that catch people out.

## 5. Open the PR

- Title is Conventional Commits and becomes the squash-merge commit and
  the CHANGELOG line. Get it right here and nothing needs rewriting.
- Fill in `.github/PULL_REQUEST_TEMPLATE.md`, including how it was
  tested.
- **Set the issue's labels and milestone on the PR explicitly.**
- Do not use a GitHub closing keyword, or word the PR as closing the
  issue, until the human has agreed it resolves it.

## 6. Stop

**Never merge.** Ask, every time, however small the change.
