# 0001. Can a failed run be resumed?

- **Status:** Complete
- **Spike:** #120
- **Time-box:** n/a (desk investigation from an existing conversation thread, not a timed spike)
- **Date:** 2026-08-24

## Verdict

New complexity. Today a run that throws an exception is caught, marked
`Exceptional` then `Finished`, and is a dead end — nothing is persisted,
and the state machine has no `Finished -> *` transition, so the only way
to retry is to start a brand-new run from scratch. Adding "save what
happened and resume it" is not a small addition: most of the run-history
data is trivial to serialize, but the `Outcome` hierarchy that same
history carries is an open, polymorphic set of types with no
serialization precedent anywhere in the codebase today, and resuming
raises a real product question (does the step that failed re-run, or get
skipped?) that has to be answered before any of the plumbing is worth
building.

## Recommendation

Split this into two tickets, in this order:

1. **Decide the replay policy first.** Write this as ADR, not code: when
   a run resumes after `Exceptional`, does the command that was running
   at the time re-execute, or does resume start from the next step? This
   determines whether resuming can duplicate side effects (e.g. a
   `_publisher.Publish` that already fired). Nothing else here should be
   built until this is answered.
2. **Then build persistence + `/resume` as its own ticket**, once (1) is
   settled, covering:
   - A rehydration path for `CliWorkflowRunState` that trusts persisted
     data directly, bypassing the normal `ChangeTo` transition
     validation (since replaying history through `ChangeTo` would
     re-validate transitions that already happened).
   - A type-discriminator/registry scheme for serializing `Outcome[]`,
     in particular `RanCliCommandOutcome`/`NextCliCommandOutcome`, which
     both wrap an open-ended `CliCommand`.
   - A serializable envelope for `ExceptionOutcome`'s raw `Exception`
     (message/type/stack as strings — not a true round-trip).
   - A `/resume` command following the existing `Exit` command template
     (factory + handler + DI registration), reusing the same DI
     resolution `CliWorkflow.CreateNewRun()` already does, but seeded
     with the rehydrated state instead of a fresh one.

## What was established

- A run that throws is caught in `CliWorkflowRun.ExecuteCommand`, moved
  to `Exceptional` then `Finished` in the same call, and returns an
  `ExceptionOutcome` — it never propagates out of the process
  (`KitCli.Workflow/Run/CliWorkflowRun.cs:99-121`). This is also the
  documented behaviour in
  [workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md).
- All run state today is in-memory only (`CliWorkflowRunState.Changes`,
  a plain `List<ICliWorkflowRunStateChange>`) — there is no run ID, no
  file, no database row, anywhere (`KitCli.Workflow/Run/State/CliWorkflowRunState.cs:10-97`).
- `Finished` has no outgoing transitions in the state machine's
  transition table (`CliWorkflowRunState.cs:102-124`) — a run that
  reaches it is permanently done. `CliWorkflow.NextRun()` would hand a
  caller back that same dead run if asked, and driving it further throws
  `ImpossibleStateChangeException`.
- The run-history payload splits into two halves for serialization
  purposes:
  - **Trivial:** `CliWorkflowRunStateChange`, `InstructionCliWorkflowRunStateChange`,
    `Instruction`, the status enum — plain data, straightforward to
    serialize.
  - **Not trivial:** `OutcomeCliWorkflowRunStateChange.Outcomes`, an
    `Outcome[]` over an open abstract-record hierarchy (13+ subtypes).
    `RanCliCommandOutcome`/`NextCliCommandOutcome` wrap arbitrary
    `CliCommand` instances with no type discriminator today, and
    `ExceptionOutcome` wraps a raw `System.Exception`, which doesn't
    round-trip through JSON cleanly.
- There is no `System.Text.Json` (or any JSON serialization) precedent
  anywhere in the KitCli solution to build on — a polymorphic converter
  for `Outcome`/`CliCommand` would be new work, not reuse.
- Wiring a new command (e.g. `/resume`) has a clear, simple template to
  copy: the `Exit` command (`KitCli.Workflow.Commands/Exit/*.cs`),
  registered via `AddCommandsFromAssembly`
  (`WorkflowCommandsServiceCollectionExtensions.cs:9-12`) and resolved
  by name through `CliWorkflowCommandProvider.GetCommand`.
- A resumed run still needs its DI-resolved collaborators re-resolved
  from the live `IServiceProvider` — `IInstructionParser`,
  `IInstructionValidator`, `ICliWorkflowCommandProvider`, `ISender`,
  `IPublisher` — these are never something you deserialize, only
  something you re-inject the same way `CliWorkflow.CreateNewRun()`
  already does (`KitCli.Workflow/CliWorkflow.cs:41-66`).

## Evidence

- `grep -rniE "resume|retry"` across the whole `KitCli` solution
  (excluding `obj`/`bin`): zero matches. Confirms no existing
  resume/retry mechanism under any name.
- `grep -rl "JsonSerializer|System.Text.Json"` across the solution:
  zero matches. Confirms no serialization precedent to reuse.
- Read `CliWorkflowRun.cs:27-42` (constructor) and `CliWorkflow.cs:41-66`
  (`CreateNewRun`) directly to confirm which dependencies are
  DI-resolved vs. held as data.
- Read `CliWorkflowRunState.cs:67-124` directly to confirm the
  transition table has no `Finished -> *` entry and that `ChangeTo`
  re-validates every transition against that table.
- Test `KitCli.Workflow.Tests/Run/CliWorkflowRunTests.cs:156-194`
  (`GivenCommandExecutionFails_WhenRespondToAsk_StateChangeBeforeFinishIsExceptional`)
  asserts the exact `Running -> Exceptional -> Finished` sequence,
  corroborating the exception-handling behaviour above independent of
  reading the production code.

## Open questions

- If a run resumes after `Exceptional`, does the command that was
  running at the point of failure re-execute, get skipped, or does the
  operator get asked? (This is the recommendation's item 1 — it's the
  open question that actually matters.)
- Is a file-based store (mirroring how plans/config are already
  persisted elsewhere in the Bright ecosystem) the right persistence
  target, or should this go through a different store? Not investigated
  here — no persistence layer exists in KitCli today to model this on.
- Do all 13+ `Outcome` subtypes need to survive a resume, or only the
  ones a real workflow actually produces before failing? Narrowing this
  would shrink the serialization surface but wasn't scoped in this pass.

## Out of scope

- Full design of the `Outcome`/`CliCommand` serialization
  scheme — this spike identifies that it's needed and where the sharp
  edges are, not the schema itself.
- Any prototyping or code changes — this was a desk investigation from
  an existing conversation, not a timed, hands-on-keyboard spike.
- Multi-run or concurrent-resume scenarios — out of scope because
  KitCli today enforces at most one active run per workflow by
  construction (see
  [workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md)),
  and nothing here suggests that should change.
