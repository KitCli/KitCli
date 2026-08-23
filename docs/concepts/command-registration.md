# Command registration

## Premise

A `CliCommand` implementation (a plain record, e.g. `SpareMoneyCommand`)
needs three things before a user can ever invoke it: a name the user types
to reach it, a `ICliCommandFactory` that builds it from a parsed
`Instruction`, and a MediatR `IRequestHandler<>` that runs it. Nothing in
`KitCli.Commands.Abstractions` requires a consumer to wire any of these up
by hand — `AddCommandsFromAssembly` (`CommandServiceCollectionExtensions.cs`)
derives all three from the command types it finds in a given assembly.

## Problem

Registering commands one at a time — one line of DI setup per command,
per factory, per handler — doesn't scale as a CLI grows, and gives every
new command author a chance to typo a name, forget a registration, or
register a factory under the wrong key. Deriving the wiring from the
command types themselves removes that whole class of mistake, but only if
the derivation rules are predictable enough for an author to reason about
what name their command will actually respond to.

## Solution

### Naming a command from its type

`CliCommand.GetInstructionName()` (`CliCommand.cs`) strips the `Command`
suffix off the type name, then inserts `-` before every uppercase letter
(except the first) and lowercases the result:

```
SpareMoneyCommand  →  SpareMoney  →  spare-money
```

A shorthand form is derived the same way but keeps only the uppercase
letters:

```
SpareMoneyCommand  →  SpareMoney  →  SM  →  sm
```

Both `spare-money` and `sm` resolve to the same command — there's no way
to opt out of the shorthand or pick a different one; it's mechanically
derived from the type name every time.

A command can additionally opt in to extra names by applying
`[CliCommandAlias("...")]` (`CliCommandAliasAttribute.cs`) to its type,
one attribute per alias:

```csharp
[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record SpareMoneyCommand : CliCommand;
```

`spare-money`, `sm`, `gimme`, and `give-me-cash` all then resolve to the
same command. Unlike the full and shorthand names, aliases are opt-in and
author-chosen — nothing about them is derived from the type name.

### Registering a factory per command type

`AddCommandFactories` (`CommandServiceCollectionExtensions.cs`) reflection-scans
the given assembly for every `CliCommand` subtype and every
`CliCommandFactory<>` subtype, then matches each command type to the
factory whose generic argument is that type:

- **Exactly one match** → that factory is registered, keyed under the
  full and shorthand instruction names plus any `[CliCommandAlias]`-declared
  names, via `AddKeyedSingleton`.
- **More than one match** → registration itself throws
  `ArgumentException("Multiple factories found for command type '...'")`
  — a command type can have at most one dedicated factory.
- **No match, but the command has a public parameterless constructor** →
  `BasicCliCommandFactory<TCommand>` is registered for it instead, so a
  command with no arguments to extract doesn't need a hand-written factory
  at all.
- **No match, no parameterless constructor** → the command silently has no
  factory. Nothing at startup reports this; it only surfaces the first
  time a user's ask resolves to that command name and no keyed
  `ICliCommandFactory` exists for it.

### Resolving a factory at runtime

Because the DI key is the *instruction name*, not the *command type*,
more than one command type can still end up keyed under the same name if
their `Command`-stripped, dashed names collide (or if a command
deliberately registers more than one factory under one name via
`CanCreateWhen`, e.g. `BasicDecisionCliCommandFactory`, to pick between
command variants at runtime based on prior artefacts). `CliWorkflowCommandProvider.GetCommand`
(`KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs`) resolves this by:

1. Fetching every `ICliCommandFactory` keyed under the instruction's name
   (`GetKeyedServices`) — zero results throws `NoCommandGeneratorException`.
2. Attaching the current instruction and artefact list to each candidate
   (`Attach`), then taking the **first** one whose `CanCreateWhen()`
   returns `true` — the same first-match-wins, registration-order-decides
   pattern used for instruction argument builders (see
   [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)) and
   `IOutcomeIoWriter` resolution (see [cli-app-host.md](cli-app-host.md)).
   No candidates matching throws `NoCommandGeneratorException` again.

## Constraints & tradeoffs

**Keyed DI narrows first; `CanCreateWhen` decides second.** Most commands
never need more than the keyed lookup — one factory per name, `CanCreateWhen`
returning `true` unconditionally. The `CanCreateWhen` tie-break exists for
the minority of commands that need to pick between variants of the same
instruction name at runtime (see `BasicDecisionCliCommandFactory`), and it
inherits all the same ambiguity risk instruction argument builders have:
whichever candidate is registered first wins, silently, if more than one
would return `true`.

**Registration-time failure for type-level ambiguity; runtime failure for
name-level ambiguity.** Two factories both targeting `SpareMoneyCommand`
fails loudly at startup. Two *different* command types that happen to
stem the same instruction name (e.g. via a naming collision) fails only
when a user's ask actually resolves to that name — later, and less
clearly, than the type-level case.

**No declared list of "commands this app supports."** Like instruction
arguments (see [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)),
there's no schema to introspect — the only way to know what a
`CliApp` responds to is to read the assembly's `CliCommand` types directly
or exercise it at runtime.

## Questions & answers

**What if I want a command name that doesn't fit the PascalCase-to-dashed
convention?**
The full and shorthand names are still entirely mechanical, off the type
name — renaming the type is the only way to change those. But you can add
any additional name via `[CliCommandAlias("...")]` without renaming the
type; see above.

**Why register both the full name and the shorthand, rather than pick one?**
So a user (or another command) can always use either — a longer, more
readable form for scripts/docs and a terse form for interactive typing —
without the command author doing anything beyond naming the type well.

**Does `BasicCliCommandFactory<TCommand>` work for every argument-free
command?**
Only if the command type has a public parameterless constructor. A command
with required constructor arguments needs a hand-written
`CliCommandFactory<TCommand>` regardless of whether it also needs to read
artefacts or arguments.

**Why key command factories by name instead of reflection-scanning them
the way artefact factories are scanned?** (see
[artefacts.md](artefacts.md)) Command resolution needs to go from a
user-typed *string* (the instruction name) to a factory — a keyed lookup
is the direct way to do that. Artefact factory resolution goes from a
runtime *outcome instance* to whichever factory's `For()` claims it, which
has no string key to look up by, only a type check — so it's necessarily
scan-and-test, not keyed lookup.

## Related concepts

- [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md) — how
  the raw ask string becomes the `Instruction` whose `Name` is looked up
  here; the same first-match-wins resolution pattern for its argument
  builders.
- [artefacts.md](artefacts.md) — the artefact list `Attach`ed to each
  factory candidate before `CanCreateWhen()` is evaluated.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — where
  `GetCommand` is actually called from, inside `RespondToAsk`.
- [0003-reflection-based-automatic-registration.md](../adr/0003-reflection-based-automatic-registration.md) —
  why command factories and MediatR handlers are discovered by assembly
  scan rather than registered by hand.
- [cli-app-host.md](cli-app-host.md) — `IOutcomeIoWriter` resolution uses
  the same first-match-wins pattern, one level further out.
- [0004-first-match-wins-resolution.md](../adr/0004-first-match-wins-resolution.md) —
  names the pattern behind `CanCreateWhen`'s resolution and its three
  other instances across KitCli.
- [0007-cli-command-alias-attribute.md](../adr/0007-cli-command-alias-attribute.md) — why
  extra instruction names are declared via a repeatable `[CliCommandAlias]`
  attribute rather than an override argument or a separate registration
  list.
