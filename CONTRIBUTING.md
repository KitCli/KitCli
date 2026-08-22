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
- **Keep PRs small: max 20 files, 10-15 preferred.** If a change is going
  to blow past that, plan the split into multiple PRs upfront, not after
  the fact.
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
- **If a linked issue exists, mirror its labels and milestone onto the
  PR.** GitHub doesn't do this automatically. Keeping both in sync means
  milestone/label filtering and progress tracking work across the PR
  list too, not just issues.
- CI (`dotnet build` + `dotnet test` across all six test projects) must be
  green before merge — this is enforced by branch protection, not
  discipline.
- No approving review is enforced while there's a single maintainer —
  they're also always the PR author, so GitHub won't let them approve
  their own PR anyway. [CODEOWNERS](CODEOWNERS) still maps areas to
  owners; turn required-review branch protection back on once a second
  maintainer joins.
- We squash-merge, so the PR title (Conventional Commits, per above) ends
  up as the commit title on `main` and the changelog line (see
  [Versioning](#versioning--releases)).
- Docs-only changes (like this file) can be committed straight to
  `main`.

## Testing

- **Build test doubles reusably from the start, not as a private nested
  class you promote later.** Put the double in the relevant test
  project's `TestHelpers/` (or equivalent) folder the first time, not
  the second.
- **Construct fixtures from real types, not hand-rolled equivalents** —
  build actual `Command`/`Outcome`/`Artefact` instances (or whatever the
  relevant abstraction is) rather than ad hoc anonymous objects standing
  in for them, so a fixture can't silently drift from the real shape.
- **Name test doubles `Test*`** (e.g. `TestCommandHandler`), not
  `Stub*`/`Fake*`/`Mock*`.

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

## User guides

A [user guide](docs/user-guides/) shows how to accomplish a task with a
consumer-facing pattern — black-box, no internal machinery. It's the
opposite of a concept doc's angle on the same subsystem: a concept doc
explains *why* the machinery works the way it does (reflection-based
registration, "last match wins," the outcome→artefact pipeline); a user
guide shows *how* to use it without needing to know any of that. If a
user guide finds itself explaining an internal reason for something,
that's a sign it should link to the concept doc instead of restating it.

**Write one when** a consumer-facing pattern exists that a new user
would reasonably reach for without caring how it's implemented —
artefacts, command reactions, continuous input, workflow commands.
**Skip it for** anything that's purely internal machinery with no
direct consumer-facing API — that's what a concept doc is for.

Copy [`docs/user-guides/0000-template.md`](docs/user-guides/0000-template.md).
Named by topic like concept docs (not numbered like ADRs). Verify every
code sample against current source before writing it down.

**Keep them current.** If your change makes an existing user guide
inaccurate, update it in the same PR.

## Investigations

An investigation ([`docs/investigations/`](docs/investigations/)) is what
a technical spike produces — the finding, not the code, since there's no
pair here to carry it forward otherwise. Number sequentially
(`000x-question.md`) from
[`0000-template.md`](docs/investigations/0000-template.md); skip it if a
reader wouldn't otherwise re-derive the finding from scratch.

Lead with the verdict — **new complexity** or **no new complexity** — not
the evidence; the latter closes the spike and opens a fresh ticket. An
investigation records what was found, not what was decided — that's an
ADR, which it may justify but doesn't replace. Ships through a PR with a
Status like any other work; durable facts about a dependency also belong
in that dependency's own docs, not only here.

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

Work bigger than a single issue goes through a pipeline biased toward
re-planning over predicting — estimates are inputs to prioritization,
not commitments to defend:

1. **WAG** — a fast, rough gut-feel estimate (in months), logged on
   [KitCli's own Ideas board](https://github.com/orgs/KitCli/projects/1)'s
   `WAG (months)` field, purely to judge whether an idea is worth
   pursuing at all. Non-binding — expected to be wrong. This board is
   owned by the KitCli org, not any individual's personal account —
   KitCli operates as its own organization, so its idea-staging lives
   inside it, not mixed in with a maintainer's personal projects.
2. **SWAG** — the same estimate, re-checked against everything else
   competing for the slot, logged in the same board's `SWAG (months)`
   field. **Setting `Priority` (`High`/`Medium`/`Low`) is mandatory at
   this point** — Status can't move to `SWAG'd / Prioritized` until
   it's set, forcing an explicit call on how the idea stacks up against
   what's already prioritized. "Prioritizing" then means
   sorting/grouping the board by `Priority` or `SWAG` — there's no
   separate roadmap artifact to keep in sync. Still non-binding: a
   relative sizing input, not a plan.
3. **New GitHub Project** — once an idea is greenlit, it graduates off
   the Ideas board into its own project.
4. **Inception spike** — plans the *next* milestone in real detail;
   everything beyond that is a rough forecast, re-planned properly once
   you actually get there (rolling-wave planning, not a full plan for
   the whole estimate up front). Refresh the Ideas board's `Validated
   Estimate (months)` field as it's learned, not just once.
5. **Backlog refinement, just-in-time** — rather than one big spike
   producing the full chronological order for an entire milestone, only
   the next handful of tickets need to be fully ordered and estimated
   at any moment. The rest of the milestone stays a loosely-ordered
   backlog, refined incrementally as work proceeds. A milestone-scale
   re-planning pass is still useful when picking up a milestone cold —
   treat its output as a starting point, not a fixed contract.

   A **spike** (a specific, scoped investigation — "should we support
   X," "what does Y actually look like") resolves to one of two
   outcomes: **new complexity found**, or **no new complexity**. On no
   new complexity, close the spike and open a fresh, cleanly-titled
   delivery-stage ticket for the actual build — don't retitle or reuse
   the spike issue in place. That new ticket gets sized in a normal
   backlog-refinement pass, not as part of the spike itself.
6. **Fixed-length iterations + end-of-iteration review** — work in
   short, regular iterations rather than open-ended milestone spans.
   At the end of each one: check what actually got done vs. planned,
   re-prioritize the backlog based on what was learned, and feed the
   iteration's actual pace back into WAG/SWAG calibration. This
   inspect-and-adapt step is what keeps the rest of the pipeline
   honest — without it, WAG/SWAG/the inception spike are just a plan
   nobody revisits.
7. **Tickets with Estimates** — the leaf/actionable tickets pulled into
   an iteration get the `Estimate` field (Fibonacci story points, not
   time) on the project board — the parent story tracks the outcome,
   not the effort to reach it. Don't second-guess an estimate just
   because a ticket is taking a while — re-estimate only on genuine
   scope change; see [SoloCAIRN's Sizing
   note](https://github.com/joshuaedwardcrowe/SoloCAIRN/blob/main/docs/03-lifecycle.md)
   for the full reasoning.

This repo follows [SoloCAIRN](https://github.com/joshuaedwardcrowe/SoloCAIRN)
for a ticket's Build-stage lifecycle, with one extension specific to
this repo, not something SoloCAIRN itself prescribes: **the GitHub
Issue itself is the story artifact** — no separate markdown file or
dedicated location. It's already written down, reviewable via
comments, and tracked through GitHub's own history.

## Milestones

When a milestone is tied to catching up to (or tracking) an external
spec or dependency version (e.g. a MediatR major bump), name it after
that version (e.g. `MediatR vX.Y.Z`), not a goal-style description
(e.g. `Update to Latest MediatR`) — a version-anchored name pins the
milestone to a concrete, checkable target and supports a version
history over time. For a feature-area milestone with no external
version to anchor to, a plain descriptive name is fine.

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
