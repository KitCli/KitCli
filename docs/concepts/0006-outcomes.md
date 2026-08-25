# 0006. Outcomes

Every command handler returns `Outcome[]`. Each outcome says one of two
things: show the user this, or change what the run does next.

```csharp
public override Task<Outcome[]> HandleCommand(MyCommand command, CancellationToken ct)
    => FinishThisCommand()
        .ByShowingTable(table)
        .ByRememberingPageSize(20)
        .EndAsync();
```

`FinishThisCommand()` starts an empty `OutcomeList`; each `By...` method
appends one outcome and returns `this`. See the API reference for the full
list of them.

## The three kinds

| Kind | Effect on the run | Examples |
|---|---|---|
| `Anonymous` | none | `SayOutcome`, `TableOutcome`, `ReactionOutcome` |
| `Reusable` | continues, keeping context for the next ask | `PageSizeOutcome`, `NextCliCommandOutcome` |

`NextCliCommandOutcome` is abstract, and is what `ByMovingToCommand` appends:
`SpecifiedNextCliCommandOutcome` when a handler names a type, and
`ProvidedNextCliCommandOutcome` when it hands over a command it built.
| `Final` | ends it | `FinalSayOutcome`, `NothingOutcome` |

**Only the last outcome decides the run's next state.** That is the rule
people get wrong: end on `ByFinallySaying` after remembering something and
the run ends, discarding what you just saved. Put the reusable outcome
last.

`Kind` decides that and nothing else. Whether a later command can query an
outcome depends on whether an artefact factory claims its type —
`AggregatorFilterOutcome` is `Anonymous` and still becomes an artefact.

## Gaps

Duplicates in one array are unhandled: return two `TableOutcome`s and
nothing merges or rejects them. No issue tracks it.

## See also

[0008-artefacts.md](0008-artefacts.md) · [0004-outcome-writing.md](0004-outcome-writing.md) ·
[0010-workflow-run-state-machine.md](0010-workflow-run-state-machine.md)
