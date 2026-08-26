# 0014. Suggesting what to run next

## What this is for

A run that reaches a reusable outcome — a list shown, a page displayed —
waits for what the user types next, and says nothing when that leads
nowhere. `[CliNextCommandIs]` gives it something to say.

## How to do it

Declare them on the command that parks the run, not the one suggested:

```csharp
[CliNextCommandIs("test-follow-up", "Pick up where /test-suggesting left off.")]
public record TestSuggestingCliCommand : CliCommand;

public record TestSuggestingOutcome() : Outcome(OutcomeKind.Reusable);

public class TestSuggestingCliCommandHandler : CliCommandHandler<TestSuggestingCliCommand>
{
    public override Task<Outcome[]> HandleCommand(TestSuggestingCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying("Ask for something that isn't a command to see what this suggests.")
            .ByResultingIn(new TestSuggestingOutcome())
            .EndAsync();
}
```

In the playground, `/test-suggesting` then `/nonsense` prints:

```

/test-follow-up
Pick up where /test-suggesting left off.
```

Give the name without a prefix character — the run adds the app's
configured one. Any name the user could type works, including a shorthand
or an [alias](0013-giving-a-command-extra-names.md).

## Common mistakes

**Declaring suggestions on a command that finishes the run.** They are
read off the last command that ran, and only while the run is parked. End
on a `Final` outcome and the next ask starts a fresh run, silent as ever.

**Writing the prefix into the name.** `[CliNextCommandIs("/next", ...)]`
renders as `//next`.

**Expecting it only for a misspelled command.** Anything the parked run
can't act on prints them — an unknown name, a command gated off by
`CanCreateWhen`, plain text with no prefix.

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  what parks a run at a reusable outcome in the first place.
- [docs/concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  where suggestions sit in the run's state machine.
