# Remembering state across separate asks

## What this is for

A lot of CLI interactions aren't one-shot — a user sets a page size,
then asks for "next page" several times; picks a filter, then runs
several different list commands that should all respect it. A
`Reusable` outcome is how a command remembers something for every
later ask in the same session, without you wiring up your own
storage.

## How to do it

Return a `Reusable` outcome from a handler, and any later command's
factory in the same run can read it back — even across completely
separate asks the user types one after another:

```csharp
public class SetPageSizeCliCommandHandler : CliCommandHandler<SetPageSizeCliCommand>
{
    public override Task<Outcome[]> HandleCommand(SetPageSizeCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByRememberingPageSize(command.PageSize)
            .ByFinallySaying($"Page size set to {command.PageSize}.")
            .EndAsync();
}
```

The next time the user types an ask that resolves to a command whose
factory needs that page size, read it back with
`GetRequiredArtefact<int>`/`GetArtefact<int>` inside a
`CliCommandFactory<T>` — see
[docs/concepts/artefacts.md](../concepts/artefacts.md) for the full
API. `OutcomeList` has a `By...` method for each built-in reusable
outcome — `ByRememberingPageSize`, `ByRememberingPageNumber`,
`ByRememberingFilter`, `ByAggregating`, and so on.

### Continuing without a fresh ask

Some reusable state isn't just "remembered for later" — it's meant to
drive the *very next* step immediately, with no new input from the
user at all. That's what `ByMovingToCommand(nextCommand)` is for; see
[chaining-commands.md](chaining-commands.md) for the full pattern.
The difference that matters here:

| You return... | What happens next |
|---|---|
| A plain `Reusable` outcome (e.g. `ByRememberingPageSize`) | The run waits for the user's next typed ask. Whatever they type next can see this outcome via an artefact. |
| `ByMovingToCommand(nextCommand)` | The run immediately executes `nextCommand` too, in the same turn — no ask needed. |
| A `Final` outcome (e.g. `ByFinallySaying`) | The run ends. |

## Common mistakes

**Expecting a `Reusable` outcome to run something immediately.** It
doesn't — it just makes state available to whatever the user asks for
*next*. If you want the next step to run without new input, that's
`ByMovingToCommand`, not a plain reusable outcome.

**Assuming remembered state survives past the current run.** A run is
the whole arc from the workflow's point of view — once it reaches a
`Final` outcome, everything it remembered is gone. Reusable state
carries across asks *within* one run, not across separate runs (e.g.
separate one-shot CLI invocations).

**Returning more than one reusable outcome that a later command reads
by type, without a distinguishing name.** If two different values in
the same run both end up as, say, `Artefact<int>`, a later command
asking for "the" `int` artefact by type alone gets whichever was set
*most recently* — not necessarily the one you meant. Give each a
`Name` if more than one could plausibly exist in the same run.

## Learn more

- [chaining-commands.md](chaining-commands.md) — the "continue without
  a fresh ask" half of this picture.
- [docs/concepts/outcomes.md](../concepts/outcomes.md) — the full
  `Outcome`/`OutcomeKind` taxonomy (`Anonymous`/`Reusable`/`Final`).
- [docs/concepts/artefacts.md](../concepts/artefacts.md) — how a
  `Reusable` outcome actually becomes something a later command's
  factory can query, and the "last match wins" rule that governs it.
- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  the state machine underneath: `ReachedReusableOutcome` vs.
  `MovePastAsk`, and exactly what makes a run keep going instead of
  ending.
