# 0002. Test coverage review

Date: 2026-08-29

**This is a snapshot, not current state.** It records what the dotCover
report said on the date above. The issue links are the only live record —
follow one before assuming a gap still stands.

## Summary

- Solution coverage was **77%** (2,705 of 3,519 statements), a figure
  flattered by the test projects counting toward it.
- **3 new findings filed** ([#220](https://github.com/KitCli/KitCli/issues/220),
  [#221](https://github.com/KitCli/KitCli/issues/221),
  [#222](https://github.com/KitCli/KitCli/issues/222)), each into the test
  milestone that already owns its surface. Most gaps the report shows were
  already on the board — see the skipped list below.
- Workflow (98%) and Instructions (94%) are in good shape; the host
  project (47%) and Commands.Abstractions (66%) hold the real debt.

## Methodology

A dotCover report from Rider (`dotnet test` over `KitCli.sln`) was walked
project by project. Every 0%-covered production type was checked against
existing issues by title search **and** against
[review 0001](0001-architectural-review.md)'s findings table before
filing — the title search alone missed one and produced a duplicate,
closed as [#223](https://github.com/KitCli/KitCli/issues/223).

## Findings

| # | Finding | Milestone |
|---|---|---|
| [#220](https://github.com/KitCli/KitCli/issues/220) | CliCommandReactionFactory's artefact accessors have no test coverage (13%) | Add Tests for Commands.Abstractions |
| [#221](https://github.com/KitCli/KitCli/issues/221) | ArtefactServiceCollectionExtensions' assembly-scanning registration has no test coverage (0/54) | Add Tests for Commands.Abstractions |
| [#222](https://github.com/KitCli/KitCli/issues/222) | HeadlessCliApp and BasicCliApp have no test coverage | Add Tests for the App Host and Scenarios |

## Skipped findings

Gaps the report shows that were already tracked, or are not worth tests:

- **CliAppBuilder at 32% / CliServiceCollectionExtensions at 0%** — the
  report's largest shipped-code gap, already
  [#26](https://github.com/KitCli/KitCli/issues/26).
- **The outcome IO writers at ~16%** — already
  [#115](https://github.com/KitCli/KitCli/issues/115).
- **DirectoryInfoInstructionArgumentBuilder at 0/13** — already
  [#40](https://github.com/KitCli/KitCli/issues/40).
- **DefaultInstructionValidator at 0/9** — the validator is dead code per
  [#21](https://github.com/KitCli/KitCli/issues/21); its tests wait on
  milestone 15's architecture verdict.
- **Aggregator.BeforeAggregation/AfterAggregation at 0%** — coverage
  arrives with [#50](https://github.com/KitCli/KitCli/issues/50).
- **Playground projects at 0/206** — manual-test scaffolding; the
  assert-nothing concern is [#29](https://github.com/KitCli/KitCli/issues/29).
- **CliIo at 0/17** — wraps the real console; low value to unit test.
- **KitCli.Tooling.Release at 0/234** — not filed as a coverage gap:
  [#224](https://github.com/KitCli/KitCli/issues/224) proposes retiring
  the tool in favour of a release skill, which supersedes testing it.
