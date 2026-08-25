# 0003. How should a chained command be selected and constructed?

- **Status:** In Review
- **Spike:** #148
- **Time-box:** 30 minutes
- **Date:** 2026-08-25

## Verdict

**No new complexity.** Routing `MoveToNext()` through `ICliCommandFactory` is
buildable with the machinery already present. The instruction a factory needs
is reachable from the run's own state, artefact conversion already happens
inside `CliWorkflowCommandProvider`, and a factory that cannot construct the
next command is an engineering error rather than a user one — so it belongs on
the existing `Running → Exceptional` edge, which `ExecuteCommand` already
routes every failure down. No new status, no new transition row. Carrying the
command type in a **new sibling outcome** rather than changing
`NextCliCommandOutcome` keeps the instance API intact and costs three one-line
call-site changes in `CliWorkflowRun`. The one thing that genuinely has to be
specified rather than assumed is what "already consumed" means when #124's
selection change lands, and that has a cleaner answer than either issue
proposed: track consumption per queued **outcome**, not per command.

## Recommendation

Close this spike and carry the work in fresh delivery tickets, in this order.
Everything here sits behind #142 (`register ICliCommandFactory as keyed
transient`) — factories are singletons holding mutable `Attach` state.

1. **Cover `MoveToNext()`'s happy path** — the remaining half of #11. Every
   step below modifies that path and it currently has no test.
2. **Register command factories under a `Type` key**, additively, in
   `AddCommandFactory`. Independently testable, no behaviour change.
3. **Introduce a shared abstraction over "the next command is queued"**, add
   `NextCliCommandTypeOutcome` alongside `NextCliCommandOutcome`, and add
   `ByMovingToCommand<TCommand>()`. Resolution goes through
   `CliWorkflowCommandProvider`, called from inside `ExecuteCommand`'s existing
   try block so a resolution failure lands as `Exceptional`. Both overloads
   stay — the instance one remains valid for a command that takes its data by
   constructor — so this slice is additive and nothing migrates.
4. **Re-file #124's selection change** against per-outcome consumption rather
   than the per-command predicate it currently proposes.
5. **Update the docs** — `chaining-commands.md`, `artefacts.md`, `outcomes.md`,
   `workflow-run-state-machine.md` — leading with `ByMovingToCommand<TCommand>()`
   as the recommended path, and keeping the instance overload documented as the
   option for constructor-passed data. The guide's examples currently teach the
   instance form exclusively.

Step 3 needs an ADR: it adds a member to `ICliWorkflowCommandProvider`, which
ships in `KitCli.Workflow.Commands`, and that is a breaking change for any
external implementer. The same ADR should record why both overloads coexist,
since two ways to queue the next command is the kind of thing a reader will
ask about later.

## What was established

- **A chained move that cannot resolve is `Exceptional`, and needs no new
  state.** `SuggestNextCommands` exists so a *user* who types something
  unresolvable mid-flow is offered the declared moves. A chained hop is
  declared by the engineer in code, so a missing or unwilling factory is a
  programming error, not a user one. `Running → Exceptional` is already in
  `PossibleStateChanges`, and `ExecuteCommand` already catches everything and
  changes to it, surfacing the `NoCommandGeneratorException` message through
  `ExceptionOutcome`. Resolution must therefore happen inside that try block.
- **A sibling outcome carrying the type is safe on the writer path.** No
  `IOutcomeIoWriter` matches `NextCliCommandOutcome` — every `CanWriteFor` is
  an exact-type check against some other outcome — and `CliApp.WriteOutcomes`
  silently skips an outcome with no writer. The first-match-wins interception
  hazard that killed the `PauseOutcome`/`SuggestionOutcome` pairing in #106
  does not apply to this outcome family.
- **The type-carrying outcome carries a type and nothing else.** No command
  instance is constructed when the hop is queued; the factory creates the
  instance at the moment the run moves to it, and that instance is what
  `RanCliCommandOutcome` then records. This is the whole point of the feature —
  an instance on the outcome is an instance the previous handler built, which
  is the thing being removed.
- **Which forces sibling, not subclass.**
  `NextCliCommandOutcome(CliCommand NextCommand)` has a required positional
  member, so a type-carrying subclass would have to fake an instance to satisfy
  it. A sibling under a shared base or interface avoids that, at the cost of
  broadening three call sites in `CliWorkflowRun` — `MoveToNext` (`:139`),
  `IsValidMovePastAsk` (`:174-176`), and `UpdateStateAfterOutcome` (`:199`).
- **"Already consumed" should be tracked per queued outcome, not per
  command.** Every `ByMovingToCommand` call produces a distinct outcome
  object, so outcome identity distinguishes two hops to the same command type
  where `RanCliCommandOutcome`'s command comparison — #124's proposed
  predicate — cannot. Specifying it this way makes the type-carrying and
  instance-carrying variants behave identically, including for a chain that
  revisits a step.
- **The originating instruction is available inside `MoveToNext()`.**
  `State.Changes` is public and `IInstructionCliWorkflowRunStateChange`
  exposes `Instruction`, so `Attach(instruction, artefacts)` can receive the
  real instruction rather than `Instruction.Empty`.
- **Artefact conversion needs no new API.** With resolution inside
  `CliWorkflowCommandProvider`, `ConvertOutcomesToArtefacts` stays private and
  the chained path sees the same artefacts the ask path sees. This only holds
  if resolution is not lifted into `CliWorkflowRun`.
- **`Type` is the unambiguous factory key.** `AddCommandFactory` registers each
  factory under a derived name, a shorthand, and every alias, so re-deriving a
  name from `TCommand` inherits every collision those keys can have. .NET
  keyed DI accepts any object as a key; the usual preference for strings and
  enums is so the key can be a constant in a `[FromKeyedServices]` attribute,
  which does not apply here — KitCli resolves through `GetKeyedServices` at
  runtime.
- **A command with constructor parameters and no dedicated
  `CliCommandFactory<T>` gets no factory registered at all**, and this cannot
  be caught at compile time — a `new()` constraint on `TCommand` would exclude
  exactly the factory-backed commands the feature exists for. Per the first
  point, failing at runtime as `Exceptional` is the correct behaviour.

## Evidence

- `KitCli.Workflow/Run/State/CliWorkflowRunState.cs:117-139` —
  `PossibleStateChanges`, including `Running → Exceptional` and
  `Exceptional → Finished`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:143-166` — `ExecuteCommand`'s
  try/catch changing to `Exceptional` and returning `ExceptionOutcome`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:124-141`, `:174-176`, `:199` — the
  three `NextCliCommandOutcome` call sites.
- `KitCli.Commands.Abstractions/Io/*.cs` — every `CanWriteFor` is an exact-type
  check; none matches `NextCliCommandOutcome`.
- `KitCli/CliApp.cs:50-59` — `WriteOutcomes`, `FirstOrDefault` then
  `writer?.Write`, skipping unmatched outcomes.
- `KitCli.Commands.Abstractions/Outcomes/Reusable/RanCliCommandOutcome.cs` —
  carries the `CliCommand` instance.
- `KitCli.Workflow.Abstractions/Run/State/Change/IInstructionCliWorkflowRunStateChange.cs`
  — public `Instruction` property.
- `KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs:22-58` — keyed lookup
  by `instruction.Name`, `FirstOrDefault(CanCreateWhen)`, private
  `ConvertOutcomesToArtefacts`.
- `KitCli.Commands.Abstractions/Extensions/CommandServiceCollectionExtensions.cs:80-104`
  — registration by derived name, shorthand and aliases; `:66-71` —
  auto-registration skipping any command without a parameterless constructor.
- `KitCli.Workflow.Commands/KitCli.Workflow.Commands.csproj:11-12` —
  `PackageId`, `Version` 1.0.10.

## Open questions

- Should `OutcomeList` reject a second next-command outcome in one list, so
  #124's silent drop becomes loud even before the selection change lands?
- Does adding a member to `ICliWorkflowCommandProvider` warrant a major
  version bump, given the release tooling only ever bumps patch (#127)?

## Out of scope

- The `[CliNextCommandIs]` attribute path and `SuggestNextCommands` beyond
  establishing they are a user-facing affordance a chain does not need.
- #124's second direction — a metadata-only attribute documenting a forward
  chain — which does not touch runtime dispatch.
- `ActivatorUtilities`, rejected on #147 before the spike because it bypasses
  `ICliCommandFactory` entirely.
- Any measurement against Bright.DataTool.Cli, the downstream app that
  prompted both issues.
