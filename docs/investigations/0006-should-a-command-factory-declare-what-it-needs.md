# 0006. Should a command factory declare what it needs instead of overriding CanCreateWhen?

- **Status:** In Review
- **Spike:** [#184](https://github.com/KitCli/KitCli/issues/184)
- **Time-box:** 45 minutes
- **Date:** 2026-08-26

## Verdict

New complexity, in the half the spike did not expect.

Declaring is the easy half. A factory says whether it applies by overriding
`CanCreateWhen()`, and all 28 of those overrides across three consumers ask the
same four questions, so a builder can replace them.

Reporting is the hard half, and it is what makes a descriptor worth having.
Today every unmet requirement becomes one message: `Did not find command factory
for X`. KitCli tried to fix that once — `MissingOutcomesCliCommand` printed a
hand-passed list of missing prerequisites under a `// TODO: Revisit strategy for
reporting missing outcomes.`, and was deleted in `d3c5c0f` on 2026-02-13 with
the revisit never done. It is a second deliverable, not a free consequence.

## Recommendation

#184 closes. A fresh parent ticket carries the build, in this order.

1. **Cover `CliCommandFactory`** ([#114](https://github.com/KitCli/KitCli/issues/114)) — its contract is what changes, and it has no tests.
2. **The builder** — `SubCommandIs`, `HasNoSubCommand`, `LastCommandWas<T>`, `RequiresArgument<T>`, `RequiresArtefact<T>`, `RequiresOneOf`, `ProducesOutcome<T>`. `OnDescribing` chains up the inheritance line. Build at registration, so startup checks it and `--help` renders without instantiating a factory.
3. **Merge identity into the same descriptor** — `OnDescribing` fills it from the factory, the [`[CliCommandAlias]`](../adr/0007-cli-command-alias-attribute.md) and [`[CliNextCommandIs]`](../adr/0008-suggest-next-commands-attribute.md) readers from the command type. Neither ADR is superseded, and a command with no factory still gets a descriptor.
4. **Default `CanCreateWhen()` to the declaration** — `abstract` becomes `virtual`; existing overrides still compile.
5. **An ADR**, since a hook on every factory is a cross-cutting pattern.
6. **The reporting path, its own ticket** — unmet requirements render as a table and travel as an exception. No factory and no attributes means no descriptor, so no table. Shared with [#183](https://github.com/KitCli/KitCli/issues/183): decide both together, or the app grows two vocabularies for "that didn't work".
7. **Docs in the same PR** — [command registration](../concepts/0001-command-registration.md), [writing a basic command](../user-guides/0001-writing-a-basic-command.md), `CHANGELOG.md`.

## What was established

- **The vocabulary is closed** — 28 implementations, no arbitrary predicates.

  | Shape | Count |
  |---|---|
  | Sub-instruction equals a named constant | 12 |
  | Sub-instruction absent | 6 |
  | A given command ran last | 6 |
  | A base class's answer `&&` an argument check | 4 |

- **Requirements compose two ways the spike did not assume** — through inheritance (`base.CanCreateWhen(…) && …`, four factories) and with `or` (`ranAggregatorCommand || ranFilterCommand`). Hence `RequiresOneOf`.
- **A descriptor must come from the factory type alone.** Factories are singletons mutated by `Attach()` ([#142](https://github.com/KitCli/KitCli/issues/142)); building one from that state would add a second piece of shared mutable state.
- **`Produces` and `Requires` cannot be joined.** A command produces outcomes, but a requirement is checked against artefact values. `ArtefactFactory<TOutcome>` hides the artefact type behind `AnonymousArtefact`, and two of the three artefacts mint their name from a runtime value. So `ProducesOutcome<T>` is declared and rendered, never checked.

## Evidence

`grep -rl "bool CanCreateWhen"` finds 23 factories in SpendfulnessCli, 4 in
`KitCli.Playground.Scenarios`, 1 in KitCli.Example.Filtering. SpendfulnessCli
predates the `Attach` rewrite, so the shapes count and the signatures do not.
`git log --diff-filter=D -- "*MissingOutcomes*"` finds the deleted report.

The borrowed shape is `Bright.DataTool.Cli`'s `ConnectorDescriptorBuilder`, after
`DbContext.OnConfiguring`. Its `SetId` and `SetName` do not carry over, because
KitCli derives a command's name from its type.

## Open questions

- Should overriding `CanCreateWhen()` stay possible? None of the 28 needs it, and an override is invisible to the descriptor, so the table would describe requirements the factory is not really deciding on. Removing it strands any consumer case the seven verbs miss.

## Out of scope

- Instruction and argument-value validation — [#183](https://github.com/KitCli/KitCli/issues/183).
- Making the produces/requires join checkable: a second type parameter on `ArtefactFactory<>`, breaking six untested implementations ([#116](https://github.com/KitCli/KitCli/issues/116)).
- Most-specific-match instead of first-match-wins, which would supersede [ADR 0004](../adr/0004-first-match-wins-resolution.md).
- Whether `ICliCommandFactory` becomes keyed transient ([#142](https://github.com/KitCli/KitCli/issues/142)).
