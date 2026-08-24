# Command registration

## Premise

Before a user can invoke a `CliCommand` record such as
`SpareMoneyCliCommand`, three things must exist:

- a name the user types to reach it,
- an `ICliCommandFactory` that builds it from a parsed `Instruction`,
- a MediatR `IRequestHandler<>` that runs it.

A consumer wires up none of them.
`AddCommandsFromAssembly` (`CommandServiceCollectionExtensions.cs`)
derives all three from the command types it finds in an assembly.

## Problem

Registering commands one at a time — a line of DI setup per command, per
factory, per handler — scales badly, and hands every new command author a
chance to typo a name, forget a registration, or key a factory wrongly.
Deriving the wiring from the command types removes that whole class of
mistake, but only if an author can predict what name their command will
answer to.

## Solution

### Naming a command from its type

`CliCommand.GetInstructionName()` (`CliCommand.cs`) removes `CliCommand`
from the type name, inserts `-` before every uppercase letter except the
first, and lowercases the result:

```
SpareMoneyCliCommand  →  SpareMoney  →  spare-money
```

The shorthand comes from the same stripped name, keeping only its
uppercase letters:

```
SpareMoneyCliCommand  →  SpareMoney  →  SM  →  sm
```

Both `spare-money` and `sm` reach the same command. Neither can be opted
out of or chosen by hand; the type name decides them every time.

The string removed is the whole word `CliCommand`, not a trailing
`Command`, and `CommandStringExtensions.ReplaceCommandSuffix` removes it
everywhere it appears rather than only at the end. Two consequences
follow:

- `SpareMoneyCommand` keeps its `Command`, becoming `spare-money-command`,
  shorthand `smc`. Name command types `...CliCommand` to get the short
  name.
- Removal reaches the middle of a name too, so `CliCommandLogCliCommand`
  becomes `log`.

A command opts in to extra names with `[CliCommandAlias("...")]`
(`CliCommandAliasAttribute.cs`), one attribute per alias:

```csharp
[CliCommandAlias("gimme")]
[CliCommandAlias("give-me-cash")]
public record SpareMoneyCliCommand : CliCommand;
```

Now `spare-money`, `sm`, `gimme`, and `give-me-cash` all reach it. Unlike
the full and shorthand names, aliases are chosen by the author; nothing
derives them from the type.

### Registering a factory per command type

`AddCommandFactories` (`CommandServiceCollectionExtensions.cs`) scans the
assembly for every `CliCommand` subtype and every `CliCommandFactory<>`
subtype, then matches each command to the factory whose generic argument
is that command:

- **Exactly one match** → `AddKeyedSingleton` registers that factory under
  the full name, the shorthand, and every `[CliCommandAlias]` name.
- **More than one match** → registration throws
  `ArgumentException("Multiple factories found for command type '...'")`.
  A command type gets at most one dedicated factory.
- **No match, but a public parameterless constructor** →
  `BasicCliCommandFactory<TCommand>` registers instead, sparing a command
  with no arguments a hand-written factory.
- **No match, no parameterless constructor** → the command gets no
  factory. Startup reports nothing; the gap surfaces the first time a
  user's ask resolves to that name and finds no keyed
  `ICliCommandFactory`.

### Resolving a factory at runtime

The DI key is the *instruction name*, not the *command type*, so several
command types can share a key — through a naming collision, or
deliberately, when a command registers variants under one name and picks
between them with `CanCreateWhen` (see `BasicDecisionCliCommandFactory`).
`CliWorkflowCommandProvider.GetCommand`
(`KitCli.Workflow.Commands/CliWorkflowCommandProvider.cs`) resolves that:

1. Fetch every `ICliCommandFactory` keyed under the instruction's name
   with `GetKeyedServices`. Zero results throws
   `NoCommandGeneratorException`.
2. `Attach` the current instruction and artefact list to each candidate,
   then take the **first** whose `CanCreateWhen()` returns `true`.
   Matching none throws `NoCommandGeneratorException` again.

That is the same first-match-wins, registration-order-decides rule used
for instruction argument builders (see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)) and
`IOutcomeIoWriter` resolution (see
[outcome-writing.md](outcome-writing.md)).

## Constraints & tradeoffs

**Keyed DI narrows first; `CanCreateWhen` decides second.** Most commands
need only the keyed lookup: one factory per name, `CanCreateWhen`
returning `true` always. The tie-break serves the minority that pick
between variants of one instruction name at runtime, and it carries the
same ambiguity risk as argument builders. Should two candidates both
return `true`, the one registered first wins, silently.

**Type-level ambiguity fails at registration; name-level ambiguity fails
at runtime.** Two factories targeting `SpareMoneyCliCommand` fail loudly
at startup. Two *different* command types stemming to one instruction name
fail only when a user's ask resolves there — later, and less clearly.
Tracked as [#19](https://github.com/KitCli/KitCli/issues/19).

**No declared list of "commands this app supports."** As with instruction
arguments (see
[instruction-parsing-pipeline.md](instruction-parsing-pipeline.md)), no
schema exists to introspect. To learn what a `CliApp` answers to, read the
assembly's `CliCommand` types or exercise it at runtime.

## Questions & answers

**What if I want a name that doesn't fit the PascalCase-to-dashed convention?**
The full and shorthand names follow the type name mechanically, so only
renaming the type changes them. `[CliCommandAlias("...")]` adds any other
name without a rename.

**Why register both the full name and the shorthand, rather than pick one?**
So a user or another command can use either: a readable form for scripts
and docs, a terse one for interactive typing. The author does nothing
beyond naming the type well.

**Does `BasicCliCommandFactory<TCommand>` work for every argument-free command?**
Only for command types with a public parameterless constructor. A command
with required constructor arguments needs a hand-written
`CliCommandFactory<TCommand>`, whether or not it also reads artefacts or
arguments.

**Why key command factories by name, when artefact factories are scanned?** (see
[artefacts.md](artefacts.md))
Command resolution starts from a user-typed *string*, and a keyed lookup
goes from string to factory directly. Artefact resolution starts from a
runtime *outcome instance* and asks which factory's `For()` claims it —
a type check with no string to key on, so it must scan and test.

## Related concepts

- [instruction-parsing-pipeline.md](instruction-parsing-pipeline.md) — how
  the raw ask becomes the `Instruction` whose `Name` is looked up here,
  and the same first-match-wins rule for its argument builders.
- [artefacts.md](artefacts.md) — the artefact list `Attach`ed to each
  candidate before `CanCreateWhen()` runs.
- [workflow-run-state-machine.md](workflow-run-state-machine.md) — where
  `GetCommand` is called from, inside `RespondToAsk`.
- [0003-reflection-based-automatic-registration.md](../adr/0003-reflection-based-automatic-registration.md) —
  why an assembly scan discovers command factories and MediatR handlers
  instead of hand registration.
- [outcome-writing.md](outcome-writing.md) — `IOutcomeIoWriter`
  resolution uses the same first-match-wins rule, one level further out.
- [0004-first-match-wins-resolution.md](../adr/0004-first-match-wins-resolution.md) —
  names the pattern behind `CanCreateWhen`'s resolution and its three
  other instances across KitCli.
- [0007-cli-command-alias-attribute.md](../adr/0007-cli-command-alias-attribute.md) —
  why a repeatable `[CliCommandAlias]` attribute declares extra names,
  rather than an override argument or a separate registration list.
