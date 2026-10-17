# 0015. Version all nine packages in lockstep

Status: Proposed
Date: 2026-08-29

Answers the question [ADR 0012](0012-derive-version-bumps-from-the-public-api-diff.md)
deferred, on [#128](https://github.com/KitCli/KitCli/issues/128).

## Context

The nine packages carry nine numbers today — `KitCli` 3.1.0,
`KitCli.Commands` 2.0.1, `KitCli.Instructions` 1.0.9 — and nothing records
why. `CONTRIBUTING.md` says they do not ship in lockstep; `CHANGELOG.md`
and `CLAUDE.md` say they publish as one unified set
([#178](https://github.com/KitCli/KitCli/issues/178)). Two of the three are
wrong whichever way this lands.

Independent versioning exists so a consumer picks up only what changed.
KitCli has no such consumer. `KitCli`'s transitive closure is all eight
other packages: it references `KitCli.Workflow`, which reaches `Commands`
and `Instructions`, which reach `Commands.Abstractions` and down to
`KitCli.Abstractions`. There is no partial install to optimise for.

The cost is already being paid in the other direction. Because `dotnet
pack` writes exact inter-package versions rather than ranges, a fix in
`KitCli.Instructions` forces a new version of the seven packages above it.
The blast radius is lockstep's; only the bookkeeping is independent's.

## Decision

Every packable project shares one version number, bumped together on every
release, whether or not that project changed.

The release tool keeps deriving the *level* from the public API diff as
ADR 0012 sets out. What changes is the scope: one level for the release,
the highest any package earned, applied to all nine. ADR 0012's
`max(own level, highest level of anything referenced)` walk collapses into
a single maximum over the set, and the dependencies-first ordering it
relies on stops mattering for versioning.

## Alternatives considered

- **Per-package, as shipped today** — its benefit needs a consumer who
  takes a subset, and the dependency graph shows there is none.
- **Per-package with version ranges** rather than exact pins, which would
  make the benefit real — NuGet supports it, `dotnet pack` does not emit
  it, and it buys untested version combinations.
- **Lockstep on a platform band**, as `Microsoft.Extensions.*` does — its
  major tracks the annual .NET release train, not API breakage. KitCli has
  no such external cadence to track.
- **Split the repo** so the packages are genuinely independent — answers
  the versioning question by deleting the framework's shape.

## Consequences

- One number to state, one to reason about, and no combination of packages
  a consumer can assemble that was never built together.
- **A one-time renumbering.** `KitCli.Instructions` jumps 1.0.9 to the
  common number. Nothing broke; the version simply stops meaning what it
  meant, and the release notes have to say so.
- A package with no change in it still ships a new version. That is the
  cost, and on this graph it is close to what already happens.
- #163 shrinks: the propagation walk becomes one maximum.
  [#162](https://github.com/KitCli/KitCli/issues/162)'s API baseline is
  untouched, since something must still detect that a break happened.
- `CHANGELOG.md` and `CLAUDE.md` become right and `CONTRIBUTING.md` wrong,
  reversing #178 — but only once the tooling ships and a release equalises
  the numbers. Until then all three describe a state that does not exist.
