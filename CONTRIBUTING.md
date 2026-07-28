# Contributing to KitCli

KitCli is a framework other people's code depends on, so the process below
optimizes for one thing above all: **every non-obvious decision has a paper
trail.** If someone asks "why does X work this way" in a year, the answer
should be a link, not archaeology.

## Before you write code

- **Bugs and small fixes** — just open a PR. No issue required for anything
  you can describe in the PR description.
- **Features, breaking changes, or anything touching more than one project**
  — open an issue first using the [feature request
  template](.github/ISSUE_TEMPLATE/feature_request.yml). Get a shape agreed
  before investing in the implementation.
- **Architectural decisions** — see [ADRs](#adrs) below.

## Branching & PRs

- Branch off `main`. No long-running branches — trunk-based, short-lived.
- One logical change per PR. If your PR description needs "and" to explain
  what it does, it's probably two PRs.
- **PR titles use [Conventional Commits](https://www.conventionalcommits.org/):**
  `<type>(scope): <description>` — `type` is one of `feat` `fix` `docs`
  `chore` `refactor` `test` `ci`; `scope` is optional and, where it applies,
  matches an [issue area](#issues) (`abstractions`, `instructions`,
  `commands`, `workflow`, `host`, `tooling`). Example:
  `feat(instructions): add quoting support to the tokenizer`. `description`
  is lowercase and imperative, no trailing period. For a breaking change,
  add `!` right before the colon —
  `refactor(host)!: rename RespondToNext to MoveToNext` — in addition to
  checking **Breaking change** in the PR body. This becomes the
  squash-merge commit title (see below), so it's also the CHANGELOG line —
  get it right here and there's nothing to rewrite later.
- Fill in the [PR template](.github/PULL_REQUEST_TEMPLATE.md) — in
  particular, link the issue if one exists, and say how you tested it
  (unit tests / a `KitCli.Playground.Scenarios` scenario / manual).
- CI (`dotnet build` + `dotnet test` across all six test projects) must be
  green before merge — this is enforced by branch protection, not
  discipline.
- At least one approving review is required. See [CODEOWNERS](CODEOWNERS)
  for who owns which area.
- We squash-merge, so the PR title (Conventional Commits, per above) ends
  up as the commit title on `main` and the changelog line (see
  [Versioning](#versioning--releases)).

## ADRs

An [ADR](docs/adr/) (Architecture Decision Record) captures a decision, its
alternatives, and its consequences — not how something works today (that's
[`docs/concepts/`](docs/concepts/)).

**Write one when you're:**
- Introducing a new cross-cutting pattern (e.g. "commands dispatch through
  MediatR")
- Changing a project/package boundary (splitting, merging, renaming a
  project)
- Making a breaking change to public API shape
- Reversing a previous ADR

**Skip it for:** bug fixes, internal refactors, anything a code comment
already explains. If you're not sure, err toward not writing one — a
15-minute decision doesn't need a permanent record, and an ADR nobody reads
because there are too many of them is worse than no ADR.

Copy [`docs/adr/0000-template.md`](docs/adr/0000-template.md), number it
sequentially, and open it in the same PR as the change it justifies (or on
its own if the decision precedes the implementation).

## Concepts

A [concept doc](docs/concepts/) explains how a subsystem works today —
narrative, examples, a Q&A — the opposite of an ADR's terse
decision-record shape. Write one for a subsystem a consumer would
reasonably need onboarding to (the outcome/artefact pipeline, the
instruction-parsing pipeline, the workflow state machine), not for
something a docstring already covers.

Copy [`docs/concepts/0000-template.md`](docs/concepts/0000-template.md).
Verify every class/method name and signature against the actual source
before writing it down — a concept doc describing aspirational behavior
that doesn't match `main` is worse than no doc at all, since nothing
flags it as wrong.

**Keep them current.** If your change makes an existing concept doc
inaccurate, update it in the same PR — don't leave the drift for someone
else to notice later.

## Issues

Every issue gets three independent labels once triaged:

| Axis | Values |
|---|---|
| **Type** | `bug` · `feature` · `tech-debt` · `docs` · `process` |
| **Area** | `area:abstractions` · `area:instructions` · `area:commands` · `area:workflow` · `area:host` · `area:tooling` |
| **Severity** | `sev:high` · `sev:medium` · `sev:low` |

Use the matching [issue template](.github/ISSUE_TEMPLATE/) — bug report,
feature request, or tech debt. There's no fixed triage meeting; an issue
should have an area label within about a week or it's fair game to close as
stale.

**Issue titles** follow a two-stage convention:

- **Idea-stage** (unvalidated, pre-WAG) — plain-language problem
  statements, e.g. "No way to X" / "Y doesn't handle Z". This is
  deliberate: an idea is a pitch for an unmet need, not yet a scoped
  unit of work.
- **Delivery-stage sub-issues** (carved out by a planning spike, see
  [Projects](#projects) below, ready to build) — Conventional Commits
  style, matching PR titles: `type(scope): description`, e.g.
  `feat(workflow): add side-effect notification handlers`. By this
  point the work is scoped, so the title should read like the commit
  that will close it.

## Projects

Work bigger than a single issue is tracked on a GitHub Projects (v2)
board rather than just a milestone — a board can carry an `Estimate`
field (story points, not time) and reflect scope a milestone alone
can't.

**Starting a new project:** open it with a planning spike first, not a
pre-built backlog. Decomposing a story-shaped issue into sub-issues
before the delivery order is actually agreed produces a breakdown that
looks plausible but can't be proven correct — e.g. proposing a command
before the data it depends on is parsed. The spike's job is to
establish that order collaboratively; only create sub-issues once it
concludes, and in the order it settles on.

**Estimates** go on the leaf/actionable sub-issues, not the parent
story issue — the parent tracks the outcome, not the effort to reach
it.

## Versioning & releases

All 9 published packages (`KitCli`, `KitCli.Abstractions`,
`KitCli.Commands`, `KitCli.Commands.Abstractions`, `KitCli.Instructions`,
`KitCli.Instructions.Abstractions`, `KitCli.Workflow`,
`KitCli.Workflow.Abstractions`, `KitCli.Workflow.Commands`) ship as **one
version number, always** — this is deliberate, not an oversight. Nothing
today consumes them independently of the bundle, so independent versioning
would provide a signal nobody reads. If that changes (a package gets a real
independent consumer), that's a decision for a new ADR, not a silent policy
drift.

Every squash-merged PR that changes behavior gets a line in
[`CHANGELOG.md`](CHANGELOG.md) under `[Unreleased]`, in [Keep a
Changelog](https://keepachangelog.com/) format.

**To cut a release:** open a PR that bumps `<Version>` in each project
that needs it, and merge it. Then run the
[`Publish`](.github/workflows/publish.yml) workflow manually from the
Actions tab (`workflow_dispatch` — it never triggers on its own). It
builds, tests, then packs and pushes each of the 9 packages at its own
currently-committed version, in dependency order, before cutting
`CHANGELOG.md`, tagging the commit (using `KitCli`'s own version — the
umbrella package), and creating a GitHub Release. Requires a
`NUGET_API_KEY` repository secret.

Note: the "one version number, always" policy above is the documented
intent, not the current reality — the 9 packages have already drifted
out of sync (see [#58](https://github.com/KitCli/KitCli/issues/58)).
The publish workflow deliberately doesn't try to force them back in
sync; it publishes whatever's committed per-project. Re-synchronizing
them (or deciding lockstep isn't the right model anymore) is a separate
decision, tracked on that issue.

## Code style

Match what's already there — this repo doesn't (yet) have an
`.editorconfig`-enforced style beyond what the compiler and existing
conventions imply. If you're touching a file, match its existing patterns
rather than introducing a new one in the same PR as an unrelated change.

## Questions

Open a [`process`](https://github.com/KitCli/KitCli/labels/process)-labeled
issue if something in this document is unclear or actively getting in the
way — this document is itself subject to the same PR process as everything
else.
