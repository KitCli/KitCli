# 0007. Chaining commands together

## What this is for

One ask should sometimes drive several commands in sequence: a multi-step
wizard, or a "list" command handing off to "show details for the first
result", without the user typing each step. `ByMovingToCommand` says which
command runs next from inside a handler, and KitCli runs it with no fresh
input.

## How to do it

Call `ByMovingToCommand<TCommand>()` **last**, before `.EndAsync()`. Name
the next command's type; its `ICliCommandFactory` builds it when the run
gets there:

```csharp
public class StartWizardCliCommandHandler : CliCommandHandler<StartWizardCliCommand>
{
    public override Task<Outcome[]> HandleCommand(StartWizardCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying("Starting the wizard...")
            .ByMovingToCommand<WizardStepOneCliCommand>()
            .EndAsync();
}

public class WizardStepOneCliCommandHandler : CliCommandHandler<WizardStepOneCliCommand>
{
    public override Task<Outcome[]> HandleCommand(WizardStepOneCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying("Step one done.")
            .ByMovingToCommand<WizardStepTwoCliCommand>()
            .EndAsync();
}

public class WizardStepTwoCliCommandHandler : CliCommandHandler<WizardStepTwoCliCommand>
{
    public override Task<Outcome[]> HandleCommand(WizardStepTwoCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying("Step two done.")
            .ByFinallySaying("Wizard complete!")
            .EndAsync();
}
```

One ask resolving to `StartWizardCliCommand` runs all three handlers. Each
step runs on its own pass of the host loop, so output appears in order
rather than at the end. Every handler needs its own `ByMovingToCommand`
to keep going, and the last needs `ByFinallySaying(...)`, or any
`Final`-kind outcome, to stop.

**End every chain with a `Final`-kind outcome.** Without one, KitCli
treats the run as reusable and waits for something else to move it
forward. See
[0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md).

## Giving the next command its data

Because a factory builds it, the next command can read everything the run
has gathered — see
[docs/concepts/0008-artefacts.md](../concepts/0008-artefacts.md). For what
*this* handler decides, pass arguments:

```csharp
return FinishThisCommand()
    .ByMovingToCommand<ShowBalanceCliCommand>(
        new NextCliCommandArgument<int>("limit", 10))
    .EndAsync();
```

`ShowBalanceCliCommandFactory` reads that with `GetRequiredArgument<int>("limit")`,
exactly as if the user had typed it.

**A command with constructor arguments needs a factory of its own.** KitCli
only auto-registers one for a command it can build with `new`, so chaining
to a command that has neither fails when the chain arrives, not when you
write it.

## Chaining to a command you built yourself

`ByMovingToCommand(command)` takes an instance instead. Its factory never
runs, so it sees none of the above — reach for it only when the command
takes its data by constructor and you already have all of it:

```csharp
.ByMovingToCommand(new WizardStepTwoCliCommand(collectedValue))
```

## Common mistakes

**Ending the chain on `ByMovingToCommand(...)`.** That queues one more
step rather than finishing. Somewhere in the chain a handler must call
`ByFinallySaying(...)`, or return another `Final` outcome, or the run
never completes.

**Expecting a one-shot invocation to run a whole chain.** A one-shot
invocation — `dotnet run -- /start-wizard`, built on `ArgsCliApp` — runs
only the *first* command the ask resolves to, never the chained steps an
interactive terminal app would drive. To finish the work in a single
one-shot call, do it in one handler instead of chaining.

**Mutating shared state across steps.** Pass data down the chain as an
argument, or read it back through an artefact when an earlier step's
outcome was `Reusable`. Never smuggle state between handlers in a static
or singleton.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  what "reusable" means, and how a chain maps onto the run's state.
- [docs/concepts/0006-outcomes.md](../concepts/0006-outcomes.md) — the full
  `Outcome` and `OutcomeKind` model this guide shows one slice of.
- [docs/concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  how `NextCliCommandOutcome` drives the run's state machine.
- [docs/concepts/0008-artefacts.md](../concepts/0008-artefacts.md) — passing data
  from an earlier step to a later one without threading it through every
  constructor.
- [docs/adr/0011-chain-to-a-command-by-type.md](../adr/0011-chain-to-a-command-by-type.md) —
  why there are two ways to name the next command.
