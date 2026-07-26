# KitCli

A .NET CLI framework: DI-driven command dispatch (MediatR), with
Commands/Outcomes/Artefacts/Workflow abstractions layered on top. 16
projects publish as a single, unified-version set of NuGet packages.

<!-- The full process for reviews, ADRs, concept docs, and issue triage
lives in the "repo-operating-model" skill — invoke it rather than
duplicating that procedure here. This file holds the standing facts a
session needs every time, not the multi-step playbook. -->

## Build & test

```
dotnet restore KitCli.sln
dotnet build KitCli.sln
dotnet test KitCli.sln
```

CI runs the same three steps on every PR and push to `main`.

## Conventions

- **Commits/PR titles**: Conventional Commits — `<type>(scope): <description>`.
  `type` ∈ `feat|fix|docs|chore|refactor|test|ci`. `scope` (optional) ∈
  `abstractions|instructions|commands|workflow|host|tooling`. Breaking
  change: `!` right before the colon. Description is lowercase,
  imperative, no trailing period. Squash-merge titles become the
  `CHANGELOG.md` line — get it right the first time.
- **ADR vs. concept doc**: an ADR (`docs/adr/`) records a decision and
  its alternatives; a concept doc (`docs/concepts/`) explains how a
  subsystem works today. Full criteria for each are in
  [`CONTRIBUTING.md`](CONTRIBUTING.md) — read that before writing either.
- **Issue labels** (three independent axes, always all three): type
  (`bug|feature|tech-debt|docs|process`) × area
  (`area:abstractions|area:instructions|area:commands|area:workflow|area:host|area:tooling`)
  × severity (`sev:high|sev:medium|sev:low`).
- **PR template triggers** (`.github/PULL_REQUEST_TEMPLATE.md`):
  breaking change or new cross-cutting pattern → ADR required in the
  same PR. Behavior change → `CHANGELOG.md` entry. Behavior a concept
  doc describes → update that doc in the same PR.

## Standing rules for Claude

- **Always ask before merging** any PR or branch in this repo, every
  time, no exceptions — even if a merge was approved earlier in the
  session. This applies regardless of how confident the change is.
- **Replying to PR review comments**: `gh` runs as the human's own
  GitHub account, so a top-level summary comment reads as them talking
  to themselves. Reply to each comment on its own thread (`gh api
  repos/{owner}/{repo}/pulls/{pr}/comments/{comment_id}/replies`), not
  with one combined comment. Prefix every reply with `🤖 **Claude:**` so
  it's clearly not the human's own voice.
- **When auditing all comments on a PR**, use `gh api --paginate` —
  the default page size (30) silently truncates results on PRs with a
  lot of back-and-forth, which can make an already-answered thread look
  unaddressed (or vice versa).
- **When a review comment raises a good idea that's out of scope for
  the current PR**, don't just acknowledge it — offer to file it as its
  own issue (using the label taxonomy above), and link back to the
  issue on that comment's thread once created.
- **Before editing an already-open PR's title, description, or diff**,
  re-read the current state first — a stale in-context copy is a common
  source of accidentally reverting someone else's edit.
