# 0014. Suggesting what to run next

## What this is for

A run that reaches a reusable outcome — a list shown, a page displayed —
waits for whatever the user types next. When that resolves to no command,
the app says nothing at all. `[CliNextCommandIs]` gives it something to
say: the moves that would have worked, each with a description.

## How to do it

Declare them on the command that reaches the reusable outcome, not on the
command being suggested, once per suggestion:

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

Run `/test-suggesting` then `/nonsense` in the playground, and the second
ask prints:

```

/test-follow-up
Pick up where /test-suggesting left off.
```

Give the name without a prefix character — the run adds the app's
configured one. Any name the user could type works, including a shorthand
or an alias
([0013-giving-a-command-extra-names.md](0013-giving-a-command-extra-names.md)).
The run stays parked either way, so the user can still type something
else.

## Common mistakes

**Declaring suggestions on a command that finishes the run.** They are
read off the last command that ran, and only while the run is parked at a
reusable outcome. End on a `Final` outcome and the next ask starts a
fresh run that fails as silently as it always did.

**Writing the prefix into the name.** `[CliNextCommandIs("/next", ...)]`
renders as `//next`.

**Relying on it to catch a typo.** It answers an ask that named no
command it could build — including one gated off by `CanCreateWhen`
([0006-gating-a-command-with-cancreatewhen.md](0006-gating-a-command-with-cancreatewhen.md)),
which prints the suggestions again. An ask that isn't an instruction at
all — plain text, no prefix — crashes the app instead
([#182](https://github.com/KitCli/KitCli/issues/182)).

## Learn more

- [0010-reusable-outcomes-and-the-workflow-run.md](0010-reusable-outcomes-and-the-workflow-run.md) —
  what parks a run at a reusable outcome in the first place.
- [docs/concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  where suggestions sit in the run's state machine.
- [docs/adr/0008-suggest-next-commands-attribute.md](../adr/0008-suggest-next-commands-attribute.md) —
  why the suggestion is an outcome with its own writer.
