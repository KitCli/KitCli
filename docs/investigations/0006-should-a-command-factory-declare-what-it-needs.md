# 0006. Should a command factory declare what it needs instead of overriding CanCreateWhen?

- **Status:** In Review
- **Spike:** [#184](https://github.com/KitCli/KitCli/issues/184)
- **Time-box:** 45 minutes
- **Date:** 2026-08-26

## Verdict

New complexity — but not in the half the spike expected.

The declaration half is *smaller* than #184 assumed. Across three consumers
there are 28 `CanCreateWhen` implementations, and every one of them is a
combination of four predicates: the sub-instruction equals a name, the
sub-instruction is absent, a given command ran last, an argument of a given
type and name is present. None needs an arbitrary lambda. The vocabulary is
closed, and a builder can express all 28 without an escape hatch being
reached for — though the escape hatch still stays, because a framework
cannot know that about code it has not seen.

The complexity is the other half. A descriptor is only worth building for
what it lets the framework *say*, and KitCli already tried to say it once:
`MissingOutcomesCliCommand` and its handler lived in
`KitCli.Workflow.Commands/MissingOutcomes/`, both carrying
`// TODO: Revisit strategy for reporting missing outcomes.`, and were deleted
in `d3c5c0f` on 2026-02-13. They took a hand-passed `string[]` of prerequisite
names and printed them — from inside `Create()`, after the factory had already
answered that it *could* create. The strategy was never revisited. This spike
is that revisit, and the reporting path is a second deliverable with its own
decision to make, not a free consequence of declaring requirements.

Two smaller things the issue did not assume, both real, both from consumer
code rather than the Playground: requirements compose through **inheritance**
(four factories `&&` a base class's answer with a local check), and they
compose with **or** (`return ranAggregatorCommand || ranFilterCommand;`). A
builder offering only a conjunction of `Requires…` calls cannot express either.

## Recommendation

#184 closes — it answered its question. A fresh parent ticket carries the
build, with these as sub-issues in this order.

1. **Cover `CliCommandFactory` first** —
   [#114](https://github.com/KitCli/KitCli/issues/114). The variants have no
   tests, and `CanCreateWhen`'s contract is what changes. Everything below
   lands on that code.
2. **The descriptor and its builder.** Six verbs cover the corpus:
   `SubCommandIs(name)`, `HasNoSubCommand()`, `LastCommandWas<T>()`,
   `RequiresArgument<T>(name)`, `RequiresArtefact<T>(name)` — the last two
   matching `AnyArgument<T>` and `AnyArtefact<T>` as they already are — plus
   `ProducesOutcome<T>()`, declared for the catalogue rather than checked; see
   below. `OnDescribing` chains through the inheritance line so a derived
   factory adds to its base's declaration rather than replacing it, and the
   builder carries an any-of group alongside the conjunction. Identity — the
   derived name,
   [`[CliCommandAlias]`](../adr/0007-cli-command-alias-attribute.md),
   [`[CliNextCommandIs]`](../adr/0008-suggest-next-commands-attribute.md) —
   stays where it is; see the open questions.
3. **Default `CanCreateWhen()` to the declaration.** It stops being `abstract`
   and becomes `virtual`, returning whether every declared requirement is met.
   This is source- and binary-compatible: every existing `override` still
   compiles, and `BasicCliCommandFactory<T>` and
   `BasicCreationCliCommandFactory<T>` keep their `sealed override … => true`.
4. **An ADR.** A hook on every factory is a cross-cutting pattern, which is
   what CONTRIBUTING asks for one for. It should say plainly that the
   descriptor does not reopen
   [ADR 0003](../adr/0003-reflection-based-automatic-registration.md) —
   registration stays reflection-driven and names stay type-derived.
5. **The reporting path, as its own ticket.** What a failed match tells the
   user, and through what — a new outcome, or the typed exception hierarchy.
   This is where the deleted `MissingOutcomes` strategy is finally revisited,
   and it overlaps [#183](https://github.com/KitCli/KitCli/issues/183) on the
   question of what a validation failure looks like to a user. Decide the two
   together or the app grows two vocabularies for "that didn't work".
6. **Docs in the same PR as the change** —
   [`0001-command-registration.md`](../concepts/0001-command-registration.md),
   [`0001-writing-a-basic-command.md`](../user-guides/0001-writing-a-basic-command.md),
   and `CHANGELOG.md`.

Build descriptors **at registration**, not lazily on first use. It gives a
startup-time check alongside the ones `AddCommandsFromAssembly` already makes,
and a catalogue that renders without instantiating a factory.

## What was established

- **The vocabulary is closed.** 28 implementations, no arbitrary predicates:

  | Shape | Count |
  |---|---|
  | Sub-instruction equals a named constant | 12 |
  | Sub-instruction absent (root command) | 6 |
  | A given command ran last | 6 |
  | Base class's answer `&&` an argument-presence check | 4 |

- **A descriptor must be derivable from the factory type alone** — no
  `Instruction`, no `Artefacts`. Factories are registered `AddKeyedSingleton`
  and mutated by `Attach()`, which
  [#142](https://github.com/KitCli/KitCli/issues/142) flags as a lifetime
  hazard. A descriptor built from attached state would be a second piece of
  shared mutable state on a singleton. Built from the type, the descriptor is
  independent of #142 rather than blocked behind it.
- **The same rule is already written two ways.** "No sub-instruction" appears
  as `SubInstructionName is null` five times and
  `string.IsNullOrEmpty(instruction.SubInstructionName)` once. A closed
  vocabulary removes the choice.
- **A requirement can be satisfiable two ways, and that lives in `Create()`
  today.** `AccountAttributeCliCommandFactory` takes an account name from an
  argument *or else* from an `Account` artefact, and its `CanCreateWhen` says
  nothing about either. This is the case that forces an any-of group into the
  builder, or leaves the fallback where it is.
- **KitCli had a missing-prerequisite report and deleted it.** Prior art, and
  the reason the reporting half is not a freebie. Its permanent home is this
  file; nothing in the tree records it any more.
- **A command produces outcomes, not artefacts, and the two do not join
  statically.** `ProducesOutcome<T>` is the honest verb — it is what a handler
  returns, and what the deleted report named
  (`nameof(AccountCliCommandOutcome)`). But a requirement is checked in
  artefact-*value* space, which is a different space again, and three things
  keep them apart: `ArtefactFactory<TOutcome>.CreateArtefact` returns
  `AnonymousArtefact`, so the artefact type never appears in a signature;
  `Outcome` carries only a `Kind` and no name, so a name declared in outcome
  space has nothing to bind to; and an artefact mints its own name during
  conversion, twice out of three times from a runtime value
  (`RanCommand.GetType().Name`, `Value.GetType().Name`). So a declared
  `Produces` can be rendered and read, but not checked against a `Requires`.
- **`GetRequiredArtefact<T>` and `GetRequiredArgument<T>` throw a bare
  `Exception`** under `// TODO: Handle better upstream` / `// TODO: Handle
  further upstream`. They are the same missing capability seen from inside
  `Create()`, and belong to step 5's ticket, not step 2's.

## Evidence

```
# The corpus. SpendfulnessCli is on the pre-rewrite API — CanCreateWhen took
# (CliInstruction, List<CliCommandArtefact>) rather than reading Attach state —
# so the shapes count here, not the signatures.
grep -rl "bool CanCreateWhen" --include="*.cs" --exclude-dir=obj --exclude-dir=bin .
#   SpendfulnessCli            23   (11 + 6 + 2 + 4 per the table)
#   KitCli.Playground.Scenarios 4   (3 last-command-was, 1 sub-instruction equals)
#   KitCli.Example.Filtering    1   (last-command-was && sub-instruction equals)

# The deleted report.
git log --diff-filter=D --name-only -- "*MissingOutcomes*"
#   d3c5c0f Made Outcome Collection Fluent   (2026-02-13)
git show d3c5c0f^:KitCli.Workflow.Commands/MissingOutcomes/MissingOutcomesCliCommandHandler.cs
#   "The following prerequisite outcomes were not returned from previous commands:"
```

Disjunction, in `MonthlySpendingCliCommandFactory`:

```csharp
return ranAggregatorCommand || ranFilterCommand;
```

Inheritance-chained composition, in
`FilterTransactionsOnPayeeNameEqualsCliCommandFactory` and three siblings:

```csharp
var previousCalledTransactionsCommandAndFilterArgumentPresent = base.CanCreateWhen(instruction, artefacts);
var payeeNameArgument = instruction.Arguments.OfType<string>(…ArgumentNames.Is);

return previousCalledTransactionsCommandAndFilterArgumentPresent && payeeNameArgument != null;
```

The three spaces a declaration has to straddle:

```csharp
public abstract record Outcome(OutcomeKind Kind);                       // no name
public abstract class ArtefactFactory<TOutcome> : IArtefactFactory       // outcome type is static
    where TOutcome : Outcome
{
    protected abstract AnonymousArtefact CreateArtefact(TOutcome outcome);   // artefact type is not
}
public record PageSizeArtefact(int PageSize) : Artefact<int>(nameof(PageSize), PageSize);
public record RanCliCommandArtefact(CliCommand RanCommand)
    : Artefact<CliCommand>(RanCommand.GetType().Name, RanCommand);      // name from a runtime value
```

The shape being borrowed is `Bright.DataTool.Cli`'s `Connector` /
`ConnectorDescriptor` / `ConnectorDescriptorBuilder`, itself modelled on
`DbContext.OnConfiguring`. Its `SetId(Guid)` and `SetName` do not carry over:
identity there serves connectors inside a serialized plan, where KitCli keys
by a name derived from the type. Its `Build`-time throws do carry over, but
belong at registration rather than first use.

## Open questions

- Does the descriptor eventually absorb identity? `SetDescription` overlaps
  `[CliNextCommandIs]`, which makes every *calling* site hand-write a
  description of the command it points at — one description per caller for
  the same command. Folding identity in supersedes ADRs 0007 and 0008 and
  forces every aliased command to grow a factory, since a command with a
  parameterless constructor gets `BasicCliCommandFactory<T>` generated and
  nobody subclasses that. Not now; worth its own question later.
- Does the any-of group go in the builder, or does the argument-or-artefact
  fallback stay in `Create()`?
- Should `Outcome` gain a name, so `ProducesOutcome<T>(outcomeName)` has
  something to bind to and reads symmetrically with the two `Requires` verbs?
  Today the name exists only after conversion, so the parameter would be
  inventing one.
- What does a failed match report, and to whom? Shared with #183.

## Out of scope

- Instruction and argument-value validation, and whether FluentValidation
  backs it — [#183](https://github.com/KitCli/KitCli/issues/183).
- Making the produces/requires join checkable, by giving `ArtefactFactory<>` a
  second type parameter. That is a breaking change across six implementations
  with no test coverage ([#116](https://github.com/KitCli/KitCli/issues/116)),
  and it only pays off once something validates a chain before running it —
  [#124](https://github.com/KitCli/KitCli/issues/124),
  [#147](https://github.com/KitCli/KitCli/issues/147),
  [#152](https://github.com/KitCli/KitCli/issues/152).
- Whether richer descriptors should replace first-match-wins with
  most-specific-match. That would supersede
  [ADR 0004](../adr/0004-first-match-wins-resolution.md) and is a separate
  decision.
- Whether `ICliCommandFactory` should become keyed transient
  ([#142](https://github.com/KitCli/KitCli/issues/142)). Registration-time
  descriptors do not depend on the answer.
