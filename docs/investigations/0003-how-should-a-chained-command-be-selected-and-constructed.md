# 0003. How should a chained command be selected and constructed?

- **Status:** In Review
- **Spike:** #148
- **Time-box:** 30 minutes
- **Date:** 2026-08-25

## Verdict

New complexity. #147 was filed as an overload on `OutcomeList`. It is five
tickets, an ADR, a prerequisite and a blocker.

The design questions all have answers now, and they are below. Answering them
removed uncertainty, not size. Building this means a new outcome type, a shared
abstraction over two of them, a default interface member on a shipped
interface, a second factory key, three call-site changes in `CliWorkflowRun`,
four rewritten guides, and #124 re-filed against a predicate it does not
currently propose. None of it is hard. There is a lot of it.

Two constraints sit outside the work itself. #11 leaves `MoveToNext()`'s happy
path untested, and every ticket below edits that path. #142 has to land first,
because factories are singletons holding mutable `Attach` state.

The SWAG of 0.25 months on #147 predates all of this. It stands as filed;
`Validated Estimate (months)` is the field that should carry what the spike
learned.

## Recommendation

Break #147 into five tickets, in this order. All of them sit behind #142.

1. Test `MoveToNext()`'s happy path, the remaining half of #11. Every step
   below edits that path and nothing covers it.
2. Register command factories under a `Type` key. Additive, testable alone,
   changes no behaviour.
3. Add the abstraction over "a command is queued", the
   `NextCliCommandTypeOutcome` sibling, and `ByMovingToCommand<TCommand>()`.
   Resolve through `CliWorkflowCommandProvider`, inside `ExecuteCommand`'s try
   block. Ship the provider's new method as a default interface member that
   throws, so external implementers still compile. Both overloads survive and
   nothing migrates.
4. Re-file #124's selection change against per-outcome consumption. Decide the
   multiple-hop guard in the same ticket; it is the same decision.
5. Rewrite `chaining-commands.md`, `artefacts.md`, `outcomes.md` and
   `workflow-run-state-machine.md` to lead on `ByMovingToCommand<TCommand>()`.
   The guide currently teaches the instance form and nothing else.

Step 3 needs an ADR. It changes public API shape, and a reader will later ask
why two ways to queue a command exist.

## What was established

- A chained move that fails to resolve is exceptional. `SuggestNextCommands`
  answers a user who typed an ask that resolves to nothing. An engineer
  declares a chained hop in code, so a missing factory is a defect.
  `Running → Exceptional` is already in `PossibleStateChanges`, and
  `ExecuteCommand` already catches everything and changes to it. Resolution
  therefore has to happen inside that try block.
- The type-carrying outcome carries a type. Nothing constructs a command when
  the hop is queued. The factory builds it when the run moves to it, and
  `RanCliCommandOutcome` records what the factory built. An instance on the
  outcome is an instance the previous handler built, which is the thing being
  removed.
- That forces a sibling rather than a subclass.
  `NextCliCommandOutcome(CliCommand NextCommand)` has a required positional
  member, and a type-carrying subclass would have to fake one. A sibling under
  a shared base costs three one-line changes in `CliWorkflowRun`: `MoveToNext`,
  `IsValidMovePastAsk`, `UpdateStateAfterOutcome`.
- No writer touches this outcome family, so a sibling is safe. Every
  `CanWriteFor` tests one exact type, none of them `NextCliCommandOutcome`, and
  `WriteOutcomes` skips an outcome no writer claims. The interception hazard
  that killed the `PauseOutcome` pairing in #106 does not reach here.
- Consumption belongs on the queued outcome. Each `ByMovingToCommand` call
  produces its own outcome object, so outcome identity separates two hops to
  one command type. `RanCliCommandOutcome` comparison, which #124 proposes,
  cannot. Specified this way, the type and instance variants behave alike, and
  a chain may revisit a step.
- `MoveToNext()` can reach the originating instruction. `State.Changes` is
  public and `IInstructionCliWorkflowRunStateChange` exposes `Instruction`, so
  `Attach` receives the real instruction rather than `Instruction.Empty`.
- Artefact conversion needs no new API, provided resolution stays inside
  `CliWorkflowCommandProvider`. `ConvertOutcomesToArtefacts` then remains
  private and the chained path sees what the ask path sees. Lifting resolution
  into `CliWorkflowRun` breaks this.
- `Type` is the unambiguous factory key. `AddCommandFactory` registers each
  factory under a derived name, a shorthand and every alias, so deriving a name
  from `TCommand` inherits every collision those keys carry. Keyed DI takes any
  object as a key. The usual preference for strings and enums buys a constant
  for `[FromKeyedServices]`, which KitCli does not use.
- A guard against a second queued hop belongs in the run. Every `By...` method
  funnels through `ByResultingIn`, so a check there catches fluent use, but
  `OutcomeList` derives from `List<Outcome>` and its `Add` is public and
  non-virtual. Sealing that inheritance would break consumers. `CliWorkflowRun`
  sees the whole history and can enforce the rule completely. The guard is also
  #124's decision: earliest-unconsumed selection makes several queued hops
  legal, so a guard written first is a rule the next ticket deletes. Throw
  `CliCommandException` with a code, per #34. Nothing in the solution logs, so
  throwing is the only loud channel.
- Adding to `ICliWorkflowCommandProvider` breaks nobody, so no version question
  arises. Every project targets `net10.0`, and Microsoft documents default
  interface members as the way to add a member to a shipped interface. A
  default that throws `NotSupportedException` surfaces as `Exceptional`, which
  matches the failure model above. The change stays additive, so
  `KitCli.Workflow.Commands` takes a patch bump, all the release CLI can
  express anyway (#127). A separate interface resolved in `CreateNewRun` would
  add a parameter to `CliWorkflowRun`'s public constructor and break eight test
  call sites instead.
- A command with constructor parameters and no `CliCommandFactory<T>` gets no
  factory at all, and no constraint catches it. `where TCommand : new()` would
  exclude the factory-backed commands this feature exists for. Failing at
  runtime as `Exceptional` is correct.

## Evidence

- `KitCli.Workflow/Run/State/CliWorkflowRunState.cs:117-139` —
  `PossibleStateChanges`, including `Running → Exceptional`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:143-166` — `ExecuteCommand` catching
  everything and returning `ExceptionOutcome`.
- `KitCli.Workflow/Run/CliWorkflowRun.cs:139`, `:174-176`, `:199` — the three
  `NextCliCommandOutcome` call sites.
- `KitCli.Commands.Abstractions/Io/*.cs` — every `CanWriteFor` tests one exact
  type; none tests `NextCliCommandOutcome`.
- `KitCli/CliApp.cs:50-59` — `WriteOutcomes` skipping unclaimed outcomes.
- `KitCli.Commands.Abstractions/Outcomes/OutcomeList.cs` — derives from
  `List<Outcome>`; every `By...` routes through `ByResultingIn`; nothing in
  `Outcomes/` throws.
- `KitCli.Commands.Abstractions/Outcomes/Reusable/RanCliCommandOutcome.cs` —
  carries the instance.
- `KitCli.Workflow.Abstractions/Run/State/Change/IInstructionCliWorkflowRunStateChange.cs`
  — public `Instruction`.
- `KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs:22-58` — keyed lookup
  by `instruction.Name`, `FirstOrDefault(CanCreateWhen)`, private
  `ConvertOutcomesToArtefacts`.
- `KitCli.Commands.Abstractions/Extensions/CommandServiceCollectionExtensions.cs:80-104`
  — registration by name, shorthand and alias; `:66-71` — auto-registration
  skipping commands without a parameterless constructor.
- `KitCli.Workflow/CliWorkflow.cs` — `CreateNewRun` resolving each dependency
  and passing it to the constructor; `new CliWorkflowRun(` appears at eight
  test call sites.
- `KitCli.Workflow.Commands/KitCli.Workflow.Commands.csproj:11-12` —
  `PackageId`, version 1.0.10; `net10.0` throughout.
- No `ILogger` appears anywhere in the solution.
- [Safely update interfaces using default interface methods](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions).

## Open questions

No design questions remain. Three opened during the spike and all three closed:
both overloads survive with the docs leading on the generic one, the
multiple-hop guard is #124's decision and belongs in the run, and the interface
addition ships as a default interface member.

What remains is procedural. A "new complexity" verdict has no agreed procedure
in CONTRIBUTING, which specifies only the "no new complexity" branch.

## Out of scope

- `[CliNextCommandIs]` and `SuggestNextCommands`, beyond establishing that a
  chain does not need them.
- #124's second direction, a metadata-only attribute documenting a forward
  chain, which leaves dispatch alone.
- `ActivatorUtilities`, rejected on #147 before the spike for bypassing
  `ICliCommandFactory`.
- Bright.DataTool.Cli, the downstream app that prompted both issues.
