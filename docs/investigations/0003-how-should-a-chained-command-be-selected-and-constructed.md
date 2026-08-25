# 0003. How should a chained command be selected and constructed?

- **Status:** In Review
- **Spike:** #148
- **Time-box:** 30 minutes
- **Date:** 2026-08-25

## Verdict

**New complexity.** Routing `MoveToNext()` through `ICliCommandFactory` is
reachable — the instruction and artefacts a factory needs are both already
available at that point — but three things sit underneath it that neither #147
nor #124 accounts for. The run's state machine has **no representable outcome
for a chained move that fails to resolve**: `MoveToNext()` enters `Running`
before dispatch, and `Running` has no edge to `InvalidMovePastAsk`, so a
missing or unwilling factory can only surface as `Exceptional`. Selection and
construction **do not compose as proposed**: #124's "no matching
`RanCliCommandOutcome` yet" predicate degrades from instance identity to type
equality the moment #147's outcome carries a type, which silently breaks any
chain visiting the same command type twice. And the "must have a factory"
precondition **cannot be enforced at compile time** without a `new()`
constraint that excludes precisely the factory-backed commands the feature
exists for. This is a milestone-shaped change to the run's state machine, not
an overload on `OutcomeList`.

## Recommendation

Do not build #147 or #124 as filed. Slice the work as:

1. **Cover `MoveToNext()`'s happy path** (#11's remaining half). Everything
   below changes that path; it currently has no test.
2. **Decide the failure semantics first**, as an ADR. A chained move that
   cannot resolve needs either a new status pair plus transition rows, or an
   explicit decision that it is `Exceptional`. This blocks both features and
   is the largest single unknown.
3. **Add `Type`-keyed factory registration** — additive, one line in
   `AddCommandFactory`, independently testable.
4. **Add the type-carrying outcome and route `MoveToNext()` through the
   provider**, once 2 and 3 land.
5. **Re-file #124 against the resulting model**, since its selection predicate
   has to be specified in terms of whatever 4 produces.

Steps 2 and 4 need an ADR each: step 2 decides a state-machine rule, step 4
changes public API shape on a shipped package.

## What was established

- **The originating instruction is available inside `MoveToNext()`.**
  `State.Changes` is public and
  `IInstructionCliWorkflowRunStateChange.Instruction` exposes the instruction
  that started the run, so `Attach(instruction, artefacts)` can be given the
  real instruction rather than `Instruction.Empty`. #147's question 3 is
  answerable without new machinery.
- **Artefact conversion needs no new API.** If resolution happens inside
  `CliWorkflowCommandProvider`, `ConvertOutcomesToArtefacts` stays private and
  the chained path gets the same artefacts the ask path gets. #147's question 4
  dissolves, provided resolution is not lifted into `CliWorkflowRun`.
- **`Type`-keyed registration is the unambiguous key.** `AddCommandFactory`
  already registers each factory under a derived name, a shorthand, and every
  alias; re-deriving a name from `TCommand` inherits every collision those
  keys can have. Registering the `Type` as an additional key does not.
- **The state machine cannot express a failed chained move.**
  `PossibleStateChanges` allows `MovePastAsk → InvalidMovePastAsk`, but
  `MoveToNext()` calls `State.ChangeTo(Running)` before dispatch, and there is
  no `Running → InvalidMovePastAsk` edge. `RespondToAsk`'s
  `NoCommandGeneratorException` fallback — which either finishes as
  `InvalidAsk` or offers `SuggestNextCommands` — has no analogue reachable
  from `MovePastAsk`, so a mid-chain resolution failure cannot suggest
  next commands the way a mid-flow ask can.
- **`InvalidMovePastAsk` already means something else.** It records a caller
  invoking `MoveToNext()` out of step with the run's history, as
  `CliWorkflowRunTerminalPathInvariantTests.ViaInvalidMovePastAsk` documents.
  Reusing it for "the next command could not be constructed" conflates a
  caller error with a resolution failure.
- **Type-carrying outcomes break instance-identity selection.**
  `RanCliCommandOutcome` carries the `CliCommand` instance. #124 proposes
  selecting the earliest `NextCliCommandOutcome` with no matching
  `RanCliCommandOutcome`; against a type that comparison cannot distinguish
  two visits to the same command type, so a chain that revisits a step is
  either stranded or re-runs.
- **The precondition is unenforceable at compile time.** A command with
  constructor parameters and no dedicated `CliCommandFactory<T>` gets no
  factory registered at all. Constraining `TCommand` to `new()` would exclude
  exactly the commands this feature targets, so "has a factory" can only fail
  at runtime.
- **`ICliWorkflowCommandProvider` ships in a published package.**
  `KitCli.Workflow.Commands` 1.0.10 carries `PackageId`, so adding a member to
  the interface breaks any external implementer.

## Evidence

- `KitCli.Workflow/Run/State/CliWorkflowRunState.cs:117-139` — the full
  `PossibleStateChanges` table. `Running` has edges to `InvalidAsk`,
  `Exceptional`, `ReachedReusableOutcome`, `MovePastAsk`, and
  `ReachedFinalOutcome`; none to `InvalidMovePastAsk`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:124-141` — `MoveToNext()` guards,
  changes to `Running`, then takes `.Last()` of every `NextCliCommandOutcome`
  and dispatches the instance directly, never touching
  `ICliWorkflowCommandProvider`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:100-118` — `RespondToAsk`'s
  `NoCommandGeneratorException` fallback, reachable only on the ask path.
- `KitCli.Workflow.Abstractions/Run/State/Change/IInstructionCliWorkflowRunStateChange.cs`
  — public `Instruction` property.
- `KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs:22-58` — keyed
  lookup by `instruction.Name`, `FirstOrDefault(CanCreateWhen)`, and the
  private `ConvertOutcomesToArtefacts`.
- `KitCli.Commands.Abstractions/Extensions/CommandServiceCollectionExtensions.cs:80-104`
  — `AddCommandFactory` registering by derived name, shorthand, and aliases;
  `:66-71` — auto-registration skipping any command without a parameterless
  constructor.
- `KitCli.Commands.Abstractions/Outcomes/Reusable/RanCliCommandOutcome.cs` —
  carries the `CliCommand` instance.
- `KitCli.Workflow.Commands/KitCli.Workflow.Commands.csproj:11-12` —
  `PackageId` and `Version` 1.0.10.

## Open questions

- Does a failed chained move deserve its own status pair, or is `Exceptional`
  the honest answer? Decided by the ADR in recommendation step 2.
- Should a mid-chain resolution failure be able to offer `SuggestNextCommands`
  the way a mid-flow ask can, and if so what transition permits it?
- Does the instance overload of `ByMovingToCommand` stay, and which does the
  documentation recommend?
- Can a chain legitimately revisit the same command type, and if so does
  selection need per-hop identity rather than type equality?
- Does #142 (`register ICliCommandFactory as keyed transient`) have to land
  first, or only before the feature ships? Not examined inside the box.

## Out of scope

- The `[CliNextCommandIs]` attribute path and `SuggestNextCommands`, beyond
  noting the fallback is unreachable from `MovePastAsk`.
- #124's second direction — a metadata-only attribute documenting a forward
  chain — which does not touch runtime dispatch and can be decided separately.
- Whether `ActivatorUtilities` could serve any part of this. Rejected on #147
  before the spike, on the grounds that it bypasses `ICliCommandFactory`
  entirely.
- Any measurement of how the change behaves in Bright.DataTool.Cli, the
  downstream app that prompted both issues.
