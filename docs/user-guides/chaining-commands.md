# Chaining commands together

## What this is for

One ask should sometimes drive several commands in sequence: a multi-step
wizard, or a "list" command handing off to "show details for the first
result", without the user typing each step. `ByMovingToCommand` queues the
next command from inside a handler, and KitCli runs it with no fresh input.

## How to do it

Call `ByMovingToCommand(nextCommand)` **last**, before `.EndAsync()`. The
next handler receives that command directly:

```csharp
public class StartWizardCliCommandHandler : CliCommandHandler<StartWizardCliCommand>
{
    public override Task<Outcome[]> HandleCommand(StartWizardCliCommand command, CancellationToken ct)
    {
        var nextCommand = new WizardStepOneCliCommand();

        return FinishThisCommand()
            .BySaying("Starting the wizard...")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
}

public class WizardStepOneCliCommandHandler : CliCommandHandler<WizardStepOneCliCommand>
{
    public override Task<Outcome[]> HandleCommand(WizardStepOneCliCommand command, CancellationToken ct)
    {
        var nextCommand = new WizardStepTwoCliCommand();

        return FinishThisCommand()
            .BySaying("Step one done.")
            .ByMovingToCommand(nextCommand)
            .EndAsync();
    }
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
rather than at the end. Chain as many steps as you need: every handler
needs its own `ByMovingToCommand(...)` to keep going, and the last needs
`ByFinallySaying(...)`, or any `Final`-kind outcome, to stop.

**End every chain with a `Final`-kind outcome.** Without one, KitCli
treats the run as reusable and waits for something else to move it
forward. See
[reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md).

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

**Mutating shared state across steps instead of passing it down the
chain.** Build the next command with the data it needs
(`new WizardStepTwoCliCommand(collectedValue)`), or read it back through
an artefact when an earlier step's outcome was `Reusable`. Never smuggle
state between handlers in a static or singleton.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  what "reusable" means, and how a chain maps onto the run's state.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — the full
  `Outcome` and `OutcomeKind` model this guide shows one slice of.
- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  how `NextCliCommandOutcome` drives the run's state machine.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — passing data
  from an earlier step to a later one without threading it through every
  constructor.
