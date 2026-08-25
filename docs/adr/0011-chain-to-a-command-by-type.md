# 0011. Chain to a command by type, and let its factory build it

Status: Proposed
Date: 2026-08-25

## Context

A handler can hand straight on to another command, so one thing the user typed runs several
commands in a row. Today the handler builds that command itself, with
`ByMovingToCommand(new ShowBalanceCliCommand(accountId))`.

A command resolved from an instruction is built by an `ICliCommandFactory`, which is handed every
artefact the run has gathered. A chained command is built from one handler's local variables. Only
one of those two can see the run. [#147](https://github.com/KitCli/KitCli/issues/147) asks that
whichever command a chain moves to is built like any other. The spike is
[#148](https://github.com/KitCli/KitCli/issues/148), written up as
[investigation 0003](../investigations/0003-how-should-a-chained-command-be-selected-and-constructed.md).

## Decision

`ByMovingToCommand<TCommand>()` names the type. The run builds the command when the chain arrives.

A chained command is an instruction — "now run `show-balance`" is what an instruction says, and
factories are keyed by instruction name. `CliWorkflowRun` builds a fresh instruction from the
configured prefix and the name derived from the type, then resolves it through the `GetCommand`
that already exists. `CliWorkflowCommandProvider` and `ICliWorkflowCommandProvider` are untouched.
`CliCommand.GetInstructionName(Type)` derives that name, and `AddCommandFactory` now calls it too,
so registration and lookup share one derivation.

`NextCliCommandOutcome` becomes the abstract base of `ProvidedNextCliCommandOutcome`, carrying a
command the handler built, and `SpecifiedNextCliCommandOutcome`, carrying a type. Keeping the
existing name on the base leaves every `OfType<NextCliCommandOutcome>()` working.

`MoveToNext` resolves in its own try/catch and hands `ExecuteCommand` a command, the way
`RespondToAsk` already does, so a command the factory cannot build becomes an `Exceptional` run.

## Alternatives considered

- **`ActivatorUtilities.CreateInstance<TCommand>`** — bypasses the factory, which is the thing asked for.
- **Keep the instance overload, document passing data through artefacts** — nothing makes the factory path happen.
- **`GetCommand(Type, ...)` on the provider as a default interface member that throws**, per investigation 0003 — a second lookup path, and a published method whose body is a throw.
- **Register factories under an extra `Type` key**, sliced as [#150](https://github.com/KitCli/KitCli/issues/150) — the full-name derivation already exists and does not collide.
- **Suggest alternatives when a chained command will not build** — suggestion is for a person who guessed wrong; a chain is written by an engineer.
- **Constrain `TCommand` to an empty constructor** — excludes exactly the commands this exists for.

## Consequences

- A chained command reads what the run gathered. Its data arrives as artefacts rather than through the previous handler's locals — a different data-passing model for chains.
- The instruction carries a prefix and a name and nothing else, answering open question 3 on #147 as `Instruction.Empty` rather than the originating ask. A factory calling `GetRequiredArgument` cannot be chained to by type.
- A command with constructor arguments and no `CliCommandFactory<T>` has no factory registered, so chaining to it throws `NoCommandGeneratorException` at runtime. No compiler check can catch it.
- `new NextCliCommandOutcome(command)` no longer compiles; it becomes `new ProvidedNextCliCommandOutcome(command)`. Reading outcomes is unaffected.
- `MoveToNext()` still takes the last one, so a handler that chains on twice loses the first. [#152](https://github.com/KitCli/KitCli/issues/152) rewrites that rule.
- Factories stay singletons holding per-command `Attach` state, and this adds a second construction path through them. [#142](https://github.com/KitCli/KitCli/issues/142) remains open.
