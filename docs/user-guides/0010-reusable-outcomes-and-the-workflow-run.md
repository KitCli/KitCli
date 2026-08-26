# 0010. Remembering state across separate asks

## What this is for

Few CLI interactions are one-shot. A user sets a page size, then asks for
"next page" several times; picks a filter, then runs several list commands
that should all respect it. A `Reusable` outcome remembers something for
every later ask in the same run, with no storage of your own.

## How to do it

Return a `Reusable` outcome from a handler. Any later command's factory in
the same run can read it back:

```csharp
public class SetPageSizeCliCommandHandler : CliCommandHandler<SetPageSizeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(SetPageSizeCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .BySaying($"Page size set to {command.PageSize}.")
            .ByRememberingPageSize(command.PageSize)
            .EndAsync();
}
```

**The `Reusable` outcome has to be last.** Only the final outcome in the
array decides what the run does next, so `ByRememberingPageSize` follows
`BySaying`. Put a `Final` outcome last instead and the run ends, taking the
page size with it.

`OutcomeList` carries a `By...` method for each built-in reusable outcome:
`ByRememberingPageSize`, `ByRememberingPageNumber`, `ByRememberingFilter`,
`ByAggregating`, and more. Read one back with `GetArtefact<int>` or
`GetRequiredArtefact<int>` inside a `CliCommandFactory<T>`.

### Continuing without a fresh ask

Some state should drive the *very next* step at once. That is a different
thing, and the difference catches people out:

| You return... | What happens next |
|---|---|
| A plain `Reusable` outcome (e.g. `ByRememberingPageSize`) | The run waits for the user's next ask, which can read this as an artefact. |
| `ByMovingToCommand<TCommand>()` | The run builds a `TCommand` through its factory and executes it too, in the same turn, with no ask. |
| A `Final` outcome (e.g. `ByFinallySaying`) | The run ends. |

## Common mistakes

**Ending the handler on `ByFinallySaying` after remembering something.**
This is the usual way to lose state you just saved. Say the confirmation
with `BySaying`, and let the reusable outcome be last.

**Expecting a `Reusable` outcome to run something at once.** It only makes
state available to whatever the user asks for *next*. To run the next step
without new input, use `ByMovingToCommand`.

**Assuming remembered state outlives the run.** Reaching a `Final` outcome
discards everything the run remembered. Reusable state carries across asks
*within* one run, never across runs — separate one-shot invocations
included.

**Returning two reusable outcomes a later command reads by type, with no
distinguishing name.** Should two values in one run both become
`Artefact<int>`, a later command asking by type alone gets whichever was
set *most recently*. Give each a `Name` whenever more than one could exist.

## Learn more

- [0007-chaining-commands.md](0007-chaining-commands.md) — the "continue without a
  fresh ask" half of this picture.
- [0009-remembering-your-own-state.md](0009-remembering-your-own-state.md) —
  remembering a value of your own, not just the built-ins.
- [../concepts/0008-artefacts.md](../concepts/0008-artefacts.md) — how a
  `Reusable` outcome becomes something a later factory can query.
- [../concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  the state machine beneath: `ReachedReusableOutcome` against `MovePastAsk`.
