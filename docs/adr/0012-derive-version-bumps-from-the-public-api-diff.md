# 0012. Derive each package's version bump from its public API diff

Status: Proposed
Date: 2026-08-25

## Context

`KitCli.Tooling.Release` exposes one bump operation, `BumpPatchVersion`, and
`ReleaseRunner` calls it for every project it decides to bump —
[#127](https://github.com/KitCli/KitCli/issues/127). No argument makes it produce anything but a
patch.

v2.0.0 is what that costs. `NextCliCommandOutcome` went from a concrete record exposing
`NextCommand` to an abstract base ([ADR 0011](0011-chain-to-a-command-by-type.md)), so
`new NextCliCommandOutcome(cmd)` stopped compiling for consumers. The tool proposed 1.0.14. The six
`<Version>` elements were written by hand instead, which puts the semver call on whoever runs the
release, weeks after the change, reading a diff.

#127 proposes deriving the bump from Conventional Commit types. That would not have worked here: the
commit was `feat(workflow): resolve a chained command through its factory (#154)`, with no `!`, so
the derived bump would have been 1.1.0. The `**Breaking:**` line in `CHANGELOG.md` was the only
signal in the repo that pointed at a major.

This ADR assumes per-package versioning, which is the behaviour shipping today but is still owed its
own decision record — [#128](https://github.com/KitCli/KitCli/issues/128). It decides how a bump is
sized, not whether packages share a number.

## Decision

Public API changes are recorded at compile time, and the release tool reads that record.

Every packable project takes `Microsoft.CodeAnalysis.PublicApiAnalyzers` with committed
`PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`. Changing public API fails the build until the
change is written into `Unshipped`, in the PR that makes it. A `*REMOVED*` line is a major, any other
line is a minor, an empty `Unshipped` over changed files is a patch. Releasing moves `Unshipped` into
`Shipped`.

Semver then lives in exactly two places. `VersionBumper.BumpPatchVersion` becomes
`Bump(project, level)`, whose `switch` over major/minor/patch is the whole arithmetic. And the
`changed || dependsOnBumped` sweep in `ReleaseRunner` becomes
`max(own level, highest level of anything referenced)` — the walk is already dependencies-first, so
every dependency's level is known when a project is reached. That `max` is what stops a package
shipping a minor that drags an incompatible dependency behind it, since `dotnet pack` emits exact
inter-package versions rather than ranges.

The `**Breaking:**` markers in `CHANGELOG.md` and the `!` in commit subjects are cross-checks, not
derivers: they fail the release when they disagree with the API diff. The disagreement is the finding
— on v2.0.0 both signals existed, contradicted each other, and nothing looked.

## Alternatives considered

- **Derive from Conventional Commits alone**, as #127 proposes — the #154 commit had no `!` and would
  have derived 1.1.0. The marker is forgettable; a build error is not.
- **Derive from `CHANGELOG.md` alone** — it is repo-wide, so a line cannot be mapped to the package
  whose API moved. It is a good assertion that a break exists, and a poor one about where.
- **Keep patch-only and hand-edit at release time** — what v2.0.0 did. It works exactly as long as the
  person releasing reads every diff since the last tag.
- **Diff the built assembly against the published nupkg at release time** — needs the network and the
  previous package, and reports after the change has landed rather than failing the PR that makes it.
- **Version all nine packages in lockstep** — moves the question rather than answering it, and belongs
  to #128.

## Consequences

- A PR that changes public API cannot compile until it records the change. That is the mechanism, and
  it is friction on every such PR.
- Adoption needs a `PublicAPI.Shipped.txt` baseline for nine packages before anything derives from it.
  It has standalone value: silent signature breaks become build failures whether or not the tool ever
  reads the files.
- **Behavioural breaks are still missed.** v1.0.13 turned a captured `Scoped` dependency into a
  startup failure and v1.0.12 made an unhandled handler exception end the session; both changed no
  signature and both shipped as patches. For these the `CHANGELOG.md` cross-check is not redundancy,
  it is the only available signal, and it only fires if a human wrote the marker.
- So the guarantee is bounded, and should be stated as such: the tool will not understate a signature
  break, and will not contradict a written `**Breaking:**`. It cannot catch a behavioural break nobody
  wrote down.
- `VersionBumper` becomes unit-testable, which it is not today — there is no assertion to make about a
  method that only ever increments.
- Sequencing the work: the baseline, then the derivation, then the release procedure. The last is a
  skill next to `repo-operating-model`, not code — including the step that bumps
  `KitCli.Tooling.Release`'s own `KitCli` pin, which is still on 1.0.12 and gets forgotten because
  nothing mechanical asks for it.
