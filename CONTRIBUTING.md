# Contributing to KitCli

KitCli is a framework other people's code depends on, so this process
optimizes for one thing: **every non-obvious decision has a paper trail.**
Asked "why does X work this way" in a year, you should answer with a
link.

## Before you write code

- **Bugs and small fixes** — open a PR, and let its description stand in
  for the issue.
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

  Squash-merge makes this title the commit on `main` and the
  CHANGELOG line (see [Versioning](#versioning--releases)). Get it right
  here and nothing needs rewriting later.
- Fill in the [PR template](.github/PULL_REQUEST_TEMPLATE.md): link the
  issue if one exists, and say how you tested — unit tests, a
  `KitCli.Playground.Scenarios` scenario, or manual.
- **Mirror a linked issue's labels and milestone onto the PR.** GitHub
  will not, and without it milestone and label filtering only works across
  issues.
- CI (`dotnet build` plus `dotnet test` across all six test projects) must
  be green before merge. Branch protection enforces it.
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

## Docs

Five kinds live in [`docs/`](docs/): ADRs, concept docs, user guides,
investigations, technology pages. Three rules cover all of them. The
[roadmap](docs/roadmap.md) is the one page belonging to no kind — see
[ADR 0014](docs/adr/0014-a-roadmap-page-outside-the-documentation-kinds.md)
for why, and add an entry to it when a release or a change of shape lands.

- **Copy the kind's `0000-template.md`** and number it sequentially,
  `000x-topic-in-kebab-case.md`, in the order a reader should meet them.
  Technology pages take their dependency's name instead.
- **Verify every name, signature and sample against source** before it
  lands. Readers trust a doc they cannot check, so a wrong one costs more
  than a missing one.
- **Keep them current in the same PR** that makes them wrong.

Everything in `docs/` is published to
[kitcli.github.io/KitCli](https://kitcli.github.io/KitCli/). The Docs
workflow builds with `--warningsAsErrors`, so a link docfx cannot resolve
fails CI. Links to a folder, or to a file outside `docs/`, are the two that
catch people out: point at the folder's first page, and use a full
`https://github.com/KitCli/KitCli/blob/main/...` URL for anything in the
repo root. Preview before pushing:

```
docfx docfx.json --serve
```

## ADRs

An [ADR](docs/adr/) records a decision, its alternatives, and its
consequences. How something works today belongs in
[`docs/concepts/`](docs/concepts/).

**Write one when you are:**

- introducing a cross-cutting pattern ("commands dispatch through MediatR")
- changing a project or package boundary
- making a breaking change to public API shape
- reversing an earlier ADR

**Skip it for** bug fixes, internal refactors, and anything a code comment
covers. When unsure, don't: a 15-minute decision needs no permanent
record, and nobody reads a folder of fifty.

Open the ADR in the PR it justifies, or alone if the decision precedes the
code.

## Concepts

A [concept doc](docs/concepts/) explains how a subsystem works today, to
someone confused about it. Write one for a subsystem a consumer needs
onboarding to — the outcome/artefact pipeline, instruction parsing, the
workflow state machine — not for something a docstring covers.

**Keep them short: under 60 lines.** Past 100 the doc holds two concepts,
or reference material that belongs in source XML docs where docfx surfaces
it. Lead with the answer rather than building to it, and never append a
Q&A that re-answers the body.

Describe the code as it is. Aspirational behaviour reads exactly like
shipped behaviour, and nothing flags the difference.

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

`toc.yml` holds the order a reader should meet them in.

## Investigations

An investigation ([`docs/investigations/`](docs/investigations/)) is what a
technical spike leaves behind: the finding. A spike's code is throwaway,
so the write-up is what survives it. Skip it when a reader could
re-derive the finding easily.

Lead with the verdict — **new complexity** or **no new complexity** — and
put the evidence under it. An investigation records what was *found*; what was
*decided* is an ADR, which an investigation may justify but never
replaces. It ships through a PR with a Status like any other work. Durable
facts about a dependency belong in that dependency's own docs too, not
only here.

## Technology

A technology page ([`docs/technology/`](docs/technology/)) is the reference
home for how KitCli uses one external dependency: which of its features
KitCli supports, and where each stops. It answers "can I do X with the
container?"; how a KitCli subsystem works belongs in
[`docs/concepts/`](docs/concepts/).

**Write one when** a dependency's behaviour shapes what consumers can
build — the DI container's lifetimes, MediatR's dispatch — and the answer
is a table a reader returns to. **Skip it for** a dependency KitCli
consumes without constraining.

Tables over prose, and every row checked against source or a runnable
sample. Name the tracking issue for each gap, or say plainly that none
exists.

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

**Area follows behaviour.** Pick the area whose behaviour the change
alters, whatever projects the diff touches. Work that adds a type
in `Commands.Abstractions` so the workflow can construct commands
differently is `area:workflow`.

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
   (`High`/`Medium`/`Low`) is mandatory.** Status reaches
   `SWAG'd / Prioritized` only once it is set, which forces an explicit
   call against what is already prioritized. Prioritizing then means
   sorting or grouping the board, which is the roadmap.
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

   On new complexity found, the issue that prompted the spike stays open
   as the parent and the build hangs off it as sub-issues, in delivery
   order. The spike issue still closes — it answered its question — and
   is still never reused.
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
   estimate against two, ideally one smaller and one larger. The table is
   the current scale, re-derived at the start of every refinement pass
   from what has merged since the last one: anchors fixed months ago size
   a repo that no longer exists. Replace the rows rather than adding to
   them, and leave estimates set under an earlier scale alone.

   Current scale, derived 2026-08-26:

   | Points | Reference | Shape |
   |---|---|---|
   | 0.5 | [#175](https://github.com/KitCli/KitCli/pull/175) | A version string bumped for a release. One file, and CI proves it. |
   | 1 | [#129](https://github.com/KitCli/KitCli/pull/129) | XML doc comments corrected on two types. You know the answer before you open the file. |
   | 2 | [#177](https://github.com/KitCli/KitCli/pull/177) | A behaviour reproduced as a playground scenario and written into the two guides that cover it. The shape is known throughout. |
   | 3 | [#112](https://github.com/KitCli/KitCli/pull/112) | One decision, settled before coding: how to tell a changed package from an unchanged one. Reaches the release tool alone. |
   | 5 | [#126](https://github.com/KitCli/KitCli/pull/126) | Scope validation at build time. A small decision with wide reach — every app the framework builds — and what it flags appears only once it runs. |
   | 8 | [#107](https://github.com/KitCli/KitCli/pull/107) | Three questions open at the start: what an unresolved ask returns, where suggestions come from, how they render. Adds public API, changes the run loop, lands an ADR. |
   | 13 | [#154](https://github.com/KitCli/KitCli/pull/154) | Construction changed for every chained command, across two packages and registration. The design settles while you build it, with test helpers and an ADR. |

This repo follows [SoloCAIRN](https://github.com/joshuaedwardcrowe/SoloCAIRN)
for a ticket's Build-stage lifecycle, with one extension SoloCAIRN itself
does not prescribe: **the GitHub Issue is the story artifact.** No separate
markdown file, no dedicated location. It is already written down,
reviewable in comments, and tracked in GitHub's own history.

## Milestones

**A milestone is a goal.** Git tags and
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

Every squash-merged PR that changes behaviour gets a line in
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
Otherwise it:

- restores, builds and tests
- exchanges a GitHub OIDC token for a one-hour NuGet key, which needs a
  `NUGET_USER` secret rather than a stored API key
- packs and pushes all 9 packages in dependency order with
  `--skip-duplicate`
- cuts `CHANGELOG.md`, tags the commit, and creates a GitHub Release from
  the `[Unreleased]` notes
- opens an auto-merging PR for the changelog edit, since `main` is
  protected

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
