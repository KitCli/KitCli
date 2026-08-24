# Contributing to KitCli

KitCli is a framework other people's code depends on, so this process
optimizes for one thing: **every non-obvious decision has a paper trail.**
Asked "why does X work this way" in a year, the answer should be a link,
not archaeology.

## Before you write code

- **Bugs and small fixes** — open a PR. No issue needed for anything the
  PR description can cover.
- **Features, breaking changes, anything touching more than one project**
  — open an issue first, using the [feature request
  template](.github/ISSUE_TEMPLATE/feature_request.yml). Agree a shape
  before building.
- **Architectural decisions** — see [ADRs](#adrs).

## Branching & PRs

- Branch off `main`. Trunk-based, short-lived, no long-running branches.
- One logical change per PR. If the description needs "and", it is two PRs.
- **Max 20 files, 10-15 preferred.** Plan a split upfront, not after.
- **Titles use [Conventional Commits](https://www.conventionalcommits.org/):**
  `<type>(scope): <description>`.
  - `type`: `feat` `fix` `docs` `chore` `refactor` `test` `ci`
  - `scope`: optional, and where it applies matches an
    [issue area](#issues) — `abstractions`, `instructions`, `commands`,
    `workflow`, `host`, `tooling`
  - `description`: lowercase, imperative, no trailing period —
    `feat(instructions): add quoting support to the tokenizer`
  - breaking: add `!` before the colon
    (`refactor(host)!: rename RespondToNext to MoveToNext`) **and** check
    **Breaking change** in the body

  We squash-merge, so this title becomes the commit on `main` and the
  CHANGELOG line (see [Versioning](#versioning--releases)). Get it right
  here and nothing needs rewriting later.
- Fill in the [PR template](.github/PULL_REQUEST_TEMPLATE.md): link the
  issue if one exists, and say how you tested — unit tests, a
  `KitCli.Playground.Scenarios` scenario, or manual.
- **Mirror a linked issue's labels and milestone onto the PR.** GitHub
  will not, and without it milestone and label filtering only works across
  issues.
- CI (`dotnet build` plus `dotnet test` across all six test projects) must
  be green before merge. Branch protection enforces this, not discipline.
- No approving review is required while there is one maintainer, who is
  also always the author — GitHub would not allow the approval anyway.
  [CODEOWNERS](CODEOWNERS) still maps areas to owners; turn required
  review back on when a second maintainer joins.
- Docs-only changes, this file included, can go straight to `main`.

## Testing

- **Build test doubles reusably from the start.** Put the double in the
  test project's `TestHelpers/` folder the first time, not the second,
  rather than as a private nested class you promote later.
- **Construct fixtures from real types.** Build actual `Command`,
  `Outcome`, or `Artefact` instances, never ad hoc anonymous objects
  standing in for them, so a fixture cannot drift from the real shape.
- **Name test doubles `Test*`** — `TestCommandHandler`, not `Stub*`,
  `Fake*`, or `Mock*`.

## ADRs

An [ADR](docs/adr/) records a decision, its alternatives, and its
consequences — not how something works today, which is
[`docs/concepts/`](docs/concepts/).

**Write one when you are:**

- introducing a cross-cutting pattern ("commands dispatch through MediatR")
- changing a project or package boundary
- making a breaking change to public API shape
- reversing an earlier ADR

**Skip it for** bug fixes, internal refactors, and anything a code comment
covers. When unsure, don't: a 15-minute decision needs no permanent
record, and too many ADRs is worse than none, because nobody reads them.

Copy [`docs/adr/0000-template.md`](docs/adr/0000-template.md), number it
sequentially, and open it in the PR it justifies — or alone, if the
decision precedes the code.

## Concepts

A [concept doc](docs/concepts/) explains how a subsystem works today, to
someone confused about it. Write one for a subsystem a consumer needs
onboarding to — the outcome/artefact pipeline, instruction parsing, the
workflow state machine — not for something a docstring covers.

**Keep them short: under 60 lines.** Past 100 the doc holds two concepts,
or reference material that belongs in source XML docs where docfx surfaces
it. Lead with the answer rather than building to it, and never append a
Q&A that re-answers the body.

Copy [`docs/concepts/0000-template.md`](docs/concepts/0000-template.md).
Verify every name and signature against source first — a doc describing
aspirational behaviour is worse than no doc, because nothing flags it
wrong.

**Keep them current.** A change that makes a concept doc inaccurate fixes
that doc in the same PR.

## User guides

A [user guide](docs/user-guides/) shows how to accomplish a task with a
consumer-facing pattern, black-box, no internal machinery. It takes the
opposite angle to a concept doc on the same subsystem: the concept doc
explains *why* the machinery works as it does, the guide shows *how* to
use it without knowing any of that. A guide explaining an internal reason
should link the concept doc instead.

**Write one when** a consumer-facing pattern exists that a new user would
reach for without caring how it works — artefacts, command reactions,
continuous input, workflow commands. **Skip it for** internal machinery
with no consumer-facing API; that is a concept doc.

Copy [`docs/user-guides/0000-template.md`](docs/user-guides/0000-template.md).
Name by topic like concept docs, not numbered like ADRs. Verify every code
sample against current source. **Keep them current**, in the same PR.

## Investigations

An investigation ([`docs/investigations/`](docs/investigations/)) is what a
technical spike produces: the finding, not the code, since no pair carries
it forward otherwise. Number sequentially (`000x-question.md`) from
[`0000-template.md`](docs/investigations/0000-template.md), and skip it
when a reader could re-derive the finding easily.

Lead with the verdict — **new complexity** or **no new complexity** — not
the evidence. An investigation records what was *found*; what was
*decided* is an ADR, which an investigation may justify but never
replaces. It ships through a PR with a Status like any other work. Durable
facts about a dependency belong in that dependency's own docs too, not
only here.

## Issues

Every triaged issue carries three independent labels:

| Axis | Values |
|---|---|
| **Type** | `bug` · `feature` · `tech-debt` · `docs` · `process` |
| **Area** | `area:abstractions` · `area:instructions` · `area:commands` · `area:workflow` · `area:host` · `area:tooling` |
| **Severity** | `sev:high` · `sev:medium` · `sev:low` |

Use the matching [issue template](.github/ISSUE_TEMPLATE/). No triage
meeting exists; an issue without an area label after about a week is fair
game to close as stale.

**Titles follow the stage the work is at:**

- **Idea-stage** (unvalidated, pre-WAG) — plain-language problem
  statements: "No way to X", "Y doesn't handle Z". An idea is a pitch for
  an unmet need, not a scoped unit of work.
- **Delivery-stage** (carved out by a planning spike, see
  [Projects](#projects), ready to build) —
  Conventional Commits, matching PR titles:
  `feat(workflow): add side-effect notification handlers`. The work is
  scoped by now, so the title should read like the commit that closes it.

## Projects

Work bigger than one issue runs through a pipeline biased toward
re-planning over predicting. Estimates are inputs to prioritization, never
commitments to defend.

1. **WAG** — a gut-feel estimate in months, on
   [KitCli's Ideas board](https://github.com/orgs/KitCli/projects/1) under
   `WAG (months)`, purely to judge whether an idea is worth pursuing.
   Expected to be wrong. The board belongs to the KitCli org rather than
   any personal account, so idea-staging lives inside the org.
2. **SWAG** — the same estimate re-checked against everything competing
   for the slot, in `SWAG (months)`. **Setting `Priority`
   (`High`/`Medium`/`Low`) is mandatory**: Status cannot reach
   `SWAG'd / Prioritized` without it, forcing an explicit call against
   what is already prioritized. Prioritizing then means sorting or
   grouping the board — there is no separate roadmap to keep in sync.
3. **The domain board** — a greenlit idea joins the board for its domain.
   One board per domain area (Instructions, Commands, Outcomes, Artefacts,
   Workflow, Packaging, Tooling & Docs), never one per idea. A board is a
   place in the codebase, so it outlives any single piece of work.
4. **Inception spike** — plans the *next* milestone in real detail.
   Everything past that is a rough forecast, re-planned on arrival
   (rolling-wave, not a full plan up front). Refresh `Validated Estimate
   (months)` as you learn, not once.
5. **Backlog refinement, just-in-time** — only the next handful of tickets
   need full ordering and estimates. The rest of the milestone stays a
   loosely-ordered backlog. A milestone-scale re-planning pass helps when
   picking one up cold; treat its output as a starting point, not a
   contract.

   A **spike** — a scoped investigation, "should we support X", "what does
   Y look like" — resolves to **new complexity found** or **no new
   complexity**. On the latter, close the spike and open a fresh,
   cleanly-titled delivery ticket for the build. Never retitle or reuse
   the spike issue in place. That new ticket is sized in a normal
   refinement pass, not by the spike.
6. **Tickets with Estimates** — leaf tickets get `Estimate` (Fibonacci
   points, not time) on their domain board; the parent story tracks the
   outcome, not the effort. Re-estimate only on genuine scope change, not
   because a ticket is taking a while — see [SoloCAIRN's Sizing
   note](https://github.com/joshuaedwardcrowe/SoloCAIRN/blob/main/docs/03-lifecycle.md).

   **Points size relatively and nothing else.** They say this issue is a
   bigger bite than that one, which is what choosing the next thing needs.
   They are deliberately not tracked against velocity, and this repo runs
   no fixed-length iterations: velocity forecasts a date or sizes an
   iteration's capacity, and a repo with no deadline and nobody waiting
   has neither. A cadence with no consumer decays into decoration.

   **Reference anchors** keep the scale honest instead. Compare each new
   estimate against two, ideally one smaller and one larger:

   | Points | Reference | Shape |
   |---|---|---|
   | 2 | [#101](https://github.com/KitCli/KitCli/pull/101) | 80 lines, one file, one test class. No design decision, no docs. |
   | 5 | [#100](https://github.com/KitCli/KitCli/pull/100) | 195 lines across 10 files. New public API, an ADR, tests. |
   | 13 | [#82](https://github.com/KitCli/KitCli/pull/82) | 812 lines across 23 files. Breaking, with an ADR and a concept doc. |

This repo follows [SoloCAIRN](https://github.com/joshuaedwardcrowe/SoloCAIRN)
for a ticket's Build-stage lifecycle, with one extension SoloCAIRN itself
does not prescribe: **the GitHub Issue is the story artifact.** No separate
markdown file, no dedicated location. It is already written down,
reviewable in comments, and tracked in GitHub's own history.

## Milestones

**A milestone is a goal, not a release.** Git tags and
[`CHANGELOG.md`](CHANGELOG.md) handle releases, never a milestone. A
milestone groups issues that together deliver one outcome, and closes when
that outcome is met.

Milestones cut **across** domain boards freely, and often do. The two
answer different questions: a board asks what part of the codebase this
is, a milestone asks what we are trying to achieve. A goal living entirely
inside one board is still a goal — the board tracks that area
indefinitely, the milestone closes.

Name it after the outcome, plain language, a verb and an object:
`Support Args CLIs`, `Output Tables to Terminal`, `Publish Packages to
NuGet`. A milestone may contain closed issues; one whose work is already
done records a goal met and shows as complete, not as wrong.

One exception: a milestone tracking an external spec or dependency version
takes that version's name (`MediatR vX.Y.Z`) rather than a goal-style
description (`Update to Latest MediatR`), pinning it to a checkable
target.

## Versioning & releases

Every squash-merged PR that changes behavior gets a line in
[`CHANGELOG.md`](CHANGELOG.md) under `[Unreleased]`, in [Keep a
Changelog](https://keepachangelog.com/) format.

Releasing has two halves: you decide the version numbers, CI does the
rest.

**1. Bump the versions** with the release CLI, itself a KitCli app:

```bash
dotnet run --project KitCli.Tooling.Release -- /release --dry-run   # report what would change
dotnet run --project KitCli.Tooling.Release -- /release             # write the bumps
```

It finds every csproj carrying both `<PackageId>` and `<Version>`, orders
them dependencies-first, and bumps a project when that project changed
since its own last release, or when anything it references is bumping.
"Last release" comes from pickaxe-searching history for the commit that
set the current `<Version>`, so no tags are involved. Pass `--publish` to
pack and push from your machine — reserve that for a broken pipeline.

**It only ever bumps the patch number**, whatever changed —
[#127](https://github.com/KitCli/KitCli/issues/127).

**2. Merge the bumps to `main`.** The `publish` job in
[`ci.yml`](.github/workflows/ci.yml) does the rest automatically on every
push to `main`. There is no separate publish workflow and nothing to
trigger by hand.

It reads `<Version>` from `KitCli/KitCli.csproj` and stops if `v{version}`
is already tagged, which is what makes an ordinary push a no-op.
Otherwise it restores, builds, tests, exchanges a GitHub OIDC token for a
one-hour NuGet key (needing a `NUGET_USER` secret, not a stored API key),
packs and pushes all 9 packages in dependency order with
`--skip-duplicate`, cuts `CHANGELOG.md`, tags the commit, creates a GitHub
Release from the `[Unreleased]` notes, and opens an auto-merging PR for the
changelog edit, since `main` is protected.

So a release is: bump, merge, watch. The tag is the interlock — nothing
publishes twice.

**Packages do not ship in lockstep.** Only what changed gets a new
version, so upgrading the umbrella still delivers lower-level fixes
through its dependencies. That replaces this file's former "one version
number, always" policy, making the drift on
[#58](https://github.com/KitCli/KitCli/issues/58) intended rather than
accidental. Which model is right is owed an ADR —
[#128](https://github.com/KitCli/KitCli/issues/128).

## Code style

Match what is already there. This repo has no `.editorconfig`-enforced
style beyond what the compiler and existing conventions imply. Touching a
file, match its patterns rather than introducing a new one in a PR about
something else.

## Questions

Open a [`process`](https://github.com/KitCli/KitCli/labels/process)-labeled
issue when something here is unclear or getting in the way. This document
goes through the same PR process as everything else.
