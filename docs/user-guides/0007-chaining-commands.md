# 0007. Chaining commands together

## What this is for

Sometimes one thing the user typed should run several commands in a row: a
multi-step wizard, or a "list" command handing off to "show details for the
first result". `ByMovingToCommand` says which command runs next from inside
a handler, and KitCli runs it with no fresh input.

## How to do it

Call `ByMovingToCommand<TCommand>()` **last**, before `.EndAsync()`. Name
the next command's *type*; its factory builds it when the run gets there:

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
            .ByFinallySaying("Wizard complete!")
            .EndAsync();
}
```

One ask resolving to `StartWizardCliCommand` runs both handlers. Each step
runs on its own pass of the host loop, so output appears in order rather
than all at the end.

**End every chain with a `Final`-kind outcome**, such as
`ByFinallySaying`. Without one, KitCli treats the run as reusable and waits
for something else to move it forward. See
[0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md).

## Giving the next command its data

Because a factory builds it, the next command can read everything the run
has gathered — see
[../concepts/0008-artefacts.md](../concepts/0008-artefacts.md). For what
*this* handler decides, pass arguments:

```csharp
return FinishThisCommand()
    .ByMovingToCommand<ShowBalanceCliCommand>(
        new NextCliCommandArgument<int>("limit", 10))
    .EndAsync();
```

`ShowBalanceCliCommandFactory` reads that with
`GetRequiredArgument<int>("limit")`, exactly as if the user had typed it.

`ByMovingToCommand(command)` takes an instance instead. Its factory never
runs, so it sees neither artefacts nor arguments — reach for it only when
the command takes its data by constructor and you already have all of it.

## Common mistakes

**Ending the chain on `ByMovingToCommand(...)`.** That queues one more step
rather than finishing. Somewhere a handler must return a `Final` outcome,
or the run never completes.

**Recursing with no way out.** A handler may hand back to its own command —
a countdown, a retry, another page — and that ends fine as long as some
pass returns a `Final` outcome. The version with no exit never returns, and
slows as it goes because each step grows the history the run reads back.
Nothing detects this
([#173](https://github.com/KitCli/KitCli/issues/173));
`/test-unending-chain` in the playground is the mistake, not the pattern.

**Expecting the chain to pause for input.** Every step runs back to back,
with no ask in between. A step that needs something from the user has to be
reached by a fresh ask — and a headless app has none (see
[0002-creating-a-headless-app.md](0002-creating-a-headless-app.md)).

**Chaining to a command with constructor arguments and no factory.** KitCli
only auto-registers a factory for a command it can build with `new`, so
this fails when the chain arrives, not when you write it.

**Mutating shared state across steps.** Pass data down as an argument, or
read it back through an artefact. Never smuggle state between handlers in a
static or a singleton.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  what "reusable" means, and how a chain maps onto the run's state.
- [../concepts/0008-artefacts.md](../concepts/0008-artefacts.md) — passing data
  from an earlier step to a later one.
- [../adr/0011-chain-to-a-command-by-type.md](../adr/0011-chain-to-a-command-by-type.md) —
  why there are two ways to name the next command.
