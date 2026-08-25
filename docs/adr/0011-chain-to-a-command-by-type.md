# 0011. Chain to a command by type, and let its factory build it

Status: Proposed
Date: 2026-08-25

## Context

A handler can hand straight on to another command, so one thing the user typed runs several
commands in a row. Today the handler builds that next command itself, with
`ByMovingToCommand(new ShowBalanceCliCommand(accountId))`.

A command resolved from an instruction is built by an `ICliCommandFactory`, which is handed every
artefact the run has gathered. A command reached by chaining is built from one handler's local
variables. The same command is constructed two ways depending on how it was reached, and only one
of them can see the run. [#147](https://github.com/KitCli/KitCli/issues/147) asks for the
guarantee that whichever command a chain moves to is built like any other. The spike behind this
is [#148](https://github.com/KitCli/KitCli/issues/148), written up as
[investigation 0003](../investigations/0003-how-should-a-chained-command-be-selected-and-constructed.md).

## Decision

`ByMovingToCommand<TCommand>()` names the type. The run builds the command when the chain arrives.

A chained command is an instruction — "now run `show-balance`" is exactly what an instruction says,
and factories are already keyed by instruction name. So `CliWorkflowRun` builds a fresh instruction
carrying the configured prefix and the name derived from the type, and resolves it through the
`GetCommand` that already exists. `CliWorkflowCommandProvider` and `ICliWorkflowCommandProvider`
are untouched, and there is no second resolution path to keep in step with the first.

`CliCommand.GetInstructionName(Type)` derives that name. `AddCommandFactory` now calls it too, so
the name a factory is registered under and the name a chain looks it up by are one derivation.

`NextCliCommandOutcome` becomes the abstract base of `ProvidedNextCliCommandOutcome`, carrying a
command the handler built, and `SpecifiedNextCliCommandOutcome`, carrying a type. Keeping the
existing name on the base leaves every `OfType<NextCliCommandOutcome>()` working, in this repo and
in a consumer's.

`MoveToNext` resolves the command in its own try/catch and hands `ExecuteCommand` a command, the
way `RespondToAsk` already does. A command the factory cannot build becomes an `Exceptional` run.

## Alternatives considered

- **`ActivatorUtilities.CreateInstance<TCommand>`** — bypasses `ICliCommandFactory` entirely, so
  factories still never see the artefacts. That is the thing being asked for.
- **Keep the instance overload and document passing data through artefacts.** Nothing makes the
  factory path happen; it stays a convention rather than a guarantee.
- **A `GetCommand(Type, ...)` on `ICliWorkflowCommandProvider`, as a default interface member that
  throws.** Investigation 0003's proposal. It adds a method to a published interface and a second
  lookup path to express what the existing `GetCommand` already expresses.
- **Register factories under an extra `Type` key**, sliced as
  [#150](https://github.com/KitCli/KitCli/issues/150). The full-name derivation already exists and
  is unambiguous, so a second key bought nothing.
- **Suggest alternatives when a chained command won't build**, as an unrecognised ask does.
  Suggestion is a kindness to a person who guessed wrong; a chain is written by an engineer, so it
  should fail loudly.
- **Constrain `TCommand` to types with an empty constructor.** Would catch a missing factory at
  compile time, and would exclude exactly the commands this exists for.

## Consequences

A chained command reads what the run gathered, which is what #147 asked for. Its data arrives as
artefacts rather than through the previous handler's locals — a different data-passing model for
chains, not a smaller version of the current one.

The instruction it is attached to carries a prefix and a name and nothing else. That was open
question 3 on #147, answered as `Instruction.Empty` over carrying the originating ask forward: an
argument the user typed was typed at the *first* command. A factory calling `GetRequiredArgument`
therefore cannot be chained to by type.

A command with constructor arguments and no `CliCommandFactory<T>` of its own is registered with no
factory at all, so chaining to it fails at runtime with `NoCommandGeneratorException`. No compiler
check can catch it, for the reason above.

There are now two ways to name the next command. This ADR is the answer to "why are there two?".

`new NextCliCommandOutcome(command)` no longer compiles — the base is abstract. Reading is
unaffected; constructing becomes `new ProvidedNextCliCommandOutcome(command)`.

`MoveToNext()` still takes the last `NextCliCommandOutcome`, so a handler that chains on twice
still has the first silently dropped. [#152](https://github.com/KitCli/KitCli/issues/152) rewrites
that rule.

Factories stay singletons holding per-command `Attach` state, and this adds a second construction
path through them. [#142](https://github.com/KitCli/KitCli/issues/142) fixes the lifetime and
remains open.
