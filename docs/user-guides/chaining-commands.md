# Chaining commands together

## What this is for

Sometimes one user ask should drive several commands in sequence —
run a multi-step wizard, or hand off from a "list" command straight
into "show details for the first result" — without asking the user to
type each step themselves. `ByMovingToCommand` queues up the next
command from inside a handler; KitCli runs it automatically, no fresh
input required.

## How to do it

Return `ByMovingToCommand(nextCommand)` as the **last** call before
`.EndAsync()`. Whatever handler runs next receives that command
directly:

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

Typing the ask that resolves to `StartWizardCliCommand` runs all
three handlers in one turn — the user only typed once. Chain as many
steps as you need; each handler just needs its own
`ByMovingToCommand(...)` call to keep going, and a normal
`ByFinallySaying(...)` (or any `Final`-kind outcome) on the last one
to stop.

**End every chain with a `Final`-kind outcome.** If the last handler
in a chain doesn't end with one, KitCli treats the run as still
"reusable" and waits for something else to move it forward — see
[reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md)
for what decides that.

## Common mistakes

**Forgetting the final step needs to actually end the run.** A chain
that ends on `ByMovingToCommand(...)` with nothing after it just
queues up one more step — eventually something in the chain has to
call `ByFinallySaying(...)` (or another `Final` outcome) instead, or
the run never reaches completion.

**Expecting a one-shot CLI invocation to run a whole chain
automatically.** A one-shot invocation (`dotnet run -- /start-wizard`,
built on `ArgsCliApp`) only ever runs the *first* command the ask
resolves to — it doesn't drive subsequent chained steps the way the
interactive terminal app does. If your command needs to run to
completion in a single one-shot call, don't rely on chaining; do the
work in one handler instead.

**Mutating shared state across steps instead of passing it through the
chain.** Build the next command with whatever data it needs
(`new WizardStepTwoCliCommand(collectedValue)`), or read it back via an
artefact if an earlier step's outcome was `Reusable` — don't reach for
a static/singleton to smuggle state between handlers.

## Learn more

- [reusable-outcomes-and-the-workflow-run.md](reusable-outcomes-and-the-workflow-run.md) —
  what "reusable" means and how a chain of commands maps onto the
  run's state underneath.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — the full
  `Outcome`/`OutcomeKind` model this guide only shows one slice of.
- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  exactly how `NextCliCommandOutcome` drives the run's internal state
  machine.
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — how to pass
  data from an earlier step to a later one without threading it
  through every command's constructor.
