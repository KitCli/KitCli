# Architectural review — 2026-07-25

## Summary

- **37 findings**, all filed as GitHub issues: **7 high**, **16 medium**,
  **14 low**.
- Every finding carries all three label axes (type/area/severity) per
  [`CONTRIBUTING.md`](../../CONTRIBUTING.md) — the full list is always
  queryable directly from GitHub:
  [open findings from this review](https://github.com/KitCli/KitCli/issues?q=is%3Aissue+created%3A2026-07-25).
- This review is what the [`CONTRIBUTING.md`](../../CONTRIBUTING.md)
  operating model, [ADRs](../adr/), and [concept docs](../concepts/) in
  this repo were built in direct response to — several of the doc/CI/test
  gaps below were fixed in the same pass that produced this review,
  before the corresponding finding was ever filed (noted where relevant).

## Methodology

Findings came from parallel review passes across independent dimensions
(DI/composition, error handling, test coverage, naming/dead code,
tooling), rather than one linear read-through — the same shape as the
`repo-operating-model` skill (`.claude/skills/repo-operating-model/`)
now codifies. Every **high**-severity finding was personally
re-verified against the actual source (exact file, exact line, exact
current behavior) before being filed — a finding that turns out wrong
after publishing is worse than one skipped.

## Findings

<!-- generated from `gh issue list` at review-doc write time — see the
live issue list linked above for current status, since issues can be
closed/re-triaged after this doc is written. -->

### High

| # | Finding | Type | Area |
|---|---|---|---|
| [#7](https://github.com/KitCli/KitCli/issues/7) | Commands.Abstractions publicly re-exposes concrete KitCli.Instructions via InstructionArgument&lt;T&gt; | tech-debt | abstractions |
| [#8](https://github.com/KitCli/KitCli/issues/8) | Instruction argument type is inferred from content, not bound to the command's declared type | bug | instructions |
| [#9](https://github.com/KitCli/KitCli/issues/9) | BoolInstructionArgumentBuilder is an unconditional catch-all, making the custom-builder extension point unreachable | bug | instructions |
| [#10](https://github.com/KitCli/KitCli/issues/10) | Duplicate argument names throw an unhandled exception outside any fault boundary | bug | instructions |
| [#11](https://github.com/KitCli/KitCli/issues/11) | CliWorkflowRun.MoveToNext has no test coverage | tech-debt | workflow |
| [#12](https://github.com/KitCli/KitCli/issues/12) | KitCli.Commands.Abstractions (63 files, the primary extension surface) has only 3 test files | tech-debt | commands |
| [#13](https://github.com/KitCli/KitCli/issues/13) | publish.rc packs with --no-build after mutating &lt;Version&gt;, risking a version/binary mismatch | bug | tooling |

### Medium

| # | Finding | Type | Area |
|---|---|---|---|
| [#14](https://github.com/KitCli/KitCli/issues/14) | KitCli.Abstractions owns the only ICliIo implementation and a direct ConsoleTables dependency | tech-debt | abstractions |
| [#15](https://github.com/KitCli/KitCli/issues/15) | "Commands.Abstractions" is ~55 concrete types and 3 interfaces | tech-debt | commands |
| [#16](https://github.com/KitCli/KitCli/issues/16) | Commands.Abstractions has a direct dependency on concrete KitCli.Instructions for one trivial record | tech-debt | commands |
| [#17](https://github.com/KitCli/KitCli/issues/17) | 16 projects for ~3,400 lines is over-fragmented | tech-debt | tooling |
| [#18](https://github.com/KitCli/KitCli/issues/18) | An outcome with no matching writer is silently dropped | bug | commands |
| [#19](https://github.com/KitCli/KitCli/issues/19) | Ambiguous command-factory resolution is decided silently by DI registration order | bug | workflow |
| [#20](https://github.com/KitCli/KitCli/issues/20) | Building one paged/filterable table needs 3 separate Command+Factory+Handler triples | tech-debt | commands |
| [#21](https://github.com/KitCli/KitCli/issues/21) | InstructionException error taxonomy is dead code; the validator never implements the check it promises | tech-debt | instructions |
| [#22](https://github.com/KitCli/KitCli/issues/22) | Numeric/date argument parsing is culture-sensitive, not pinned to invariant culture | bug | instructions |
| [#23](https://github.com/KitCli/KitCli/issues/23) | Run and state-change history grow forever, with no eviction | tech-debt | workflow |
| [#24](https://github.com/KitCli/KitCli/issues/24) | Console.CancelKeyPress mutates shared state from a different thread than the main loop | bug | abstractions |
| [#25](https://github.com/KitCli/KitCli/issues/25) | KitCli.Abstractions.Tests tests one string extension; the table-rendering core is untested | tech-debt | abstractions |
| [#26](https://github.com/KitCli/KitCli/issues/26) | CliAppBuilder and CliServiceCollectionExtensions — the first API surface a new adopter touches — have zero test coverage | tech-debt | host |
| [#27](https://github.com/KitCli/KitCli/issues/27) | publish.rc unconditionally overwrites NUGET_API_KEY with its own placeholder | bug | tooling |
| [#28](https://github.com/KitCli/KitCli/issues/28) | README is a single sentence — no quick start, no package guidance, no link to docs/concepts | docs | tooling |
| [#29](https://github.com/KitCli/KitCli/issues/29) | Playground.Scenarios are the closest thing to integration tests but assert nothing | tech-debt | tooling |

### Low

| # | Finding | Type | Area |
|---|---|---|---|
| [#30](https://github.com/KitCli/KitCli/issues/30) | 9 packages publish in permanent lockstep — the split delivers none of the benefit of separate packages | tech-debt | tooling |
| [#31](https://github.com/KitCli/KitCli/issues/31) | No central package version management — NUnit/analyzer versions have already drifted across test projects | tech-debt | tooling |
| [#32](https://github.com/KitCli/KitCli/issues/32) | KitCli.csproj pins a direct MediatR reference nothing in the project uses | tech-debt | host |
| [#33](https://github.com/KitCli/KitCli/issues/33) | KitCli.Instructions.csproj grants InternalsVisibleTo twice, once to a project that doesn't exist | tech-debt | instructions |
| [#34](https://github.com/KitCli/KitCli/issues/34) | Bare System.Exception thrown throughout despite a typed CliException hierarchy already in use | tech-debt | commands |
| [#35](https://github.com/KitCli/KitCli/issues/35) | Filename/type-name mismatches indicate incomplete refactors | tech-debt | commands |
| [#36](https://github.com/KitCli/KitCli/issues/36) | NoInstructionException and UnknownOutcomeException are fully built and never thrown | tech-debt | commands |
| [#37](https://github.com/KitCli/KitCli/issues/37) | FinalSayOutcome's message property is named "Something", not "Message" | tech-debt | commands |
| [#38](https://github.com/KitCli/KitCli/issues/38) | ArgumentNullException constructor misuse produces a garbled diagnostic message | bug | instructions |
| [#39](https://github.com/KitCli/KitCli/issues/39) | Argument tokenizer has no quoting/escaping — a literal '--' inside a value corrupts parsing | bug | instructions |
| [#40](https://github.com/KitCli/KitCli/issues/40) | No test exists for DirectoryInfoInstructionArgumentBuilder — the builder implicated in the type-misclassification bug | tech-debt | instructions |
| [#41](https://github.com/KitCli/KitCli/issues/41) | Public enum ClIWorkflowRunStateStatus has a typo in its own name | tech-debt | workflow |
| [#42](https://github.com/KitCli/KitCli/issues/42) | CliWorkflow.NextRun() relies on an unenforced 'at most one active run' invariant | tech-debt | workflow |
| [#43](https://github.com/KitCli/KitCli/issues/43) | KitCli.Workflow.Commands is a separately versioned package with exactly one command and one production consumer | tech-debt | workflow |

## Skipped findings

Two findings surfaced during review were not filed, because the same
session's work resolved them before filing was needed:

- **No CI on PRs** — resolved by the CI workflow added in the
  operating-model PR ([`docs(tooling): add contributor operating
  model`](https://github.com/KitCli/KitCli/pull/3)) before this review's
  issues were filed.
- **Lockstep versioning with no changelog** — the changelog half of this
  was resolved by `CHANGELOG.md` in the same PR; the lockstep-versioning
  concern itself is tracked as [#30](https://github.com/KitCli/KitCli/issues/30)
  since it's a real, still-open design tradeoff, not a gap.

## Next review

Gated on ≥1,000 lines of `.cs` source changed **and** ≥6 months
elapsed since this review — see
[`.claude/skills/repo-operating-model/SKILL.md`](../../.claude/skills/repo-operating-model/SKILL.md#1-check-whether-a-full-review-is-actually-due).
