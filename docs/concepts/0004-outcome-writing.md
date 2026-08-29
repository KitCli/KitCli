# 0004. Outcome writing

A handler returns outcomes; it never prints. Turning one outcome into
output is an `IOutcomeIoWriter`'s job, and separating the two is what lets
you restyle a message without touching the command that produced it.

`CliApp.WriteOutcomes` matches each outcome against the writer list and
takes the **first** whose `CanWriteFor` returns `true` — the same
first-match-wins rule used for command factories and argument builders.

```csharp
public interface IOutcomeIoWriter
{
    bool CanWriteFor(Outcome outcome);
    void Write(Outcome outcome);
}
```

`AddCommandAbstractions` registers eight built-ins, each claiming exactly
one outcome type and writing through `ICliIo`. `CliAppBuilder.Run` resolves
them once from the root provider, so DI registration order becomes match
order — and a writer taking a `Scoped` dependency fails at startup. Give it
`ICliIo`, or read per-run data off the `Outcome` itself. Because no two
built-ins claim the same type, first-match-wins decides nothing today; it
is the rule waiting for the first writer that claims a base type.

## Silence is often deliberate

Half the outcome types have no writer at all — `RanCliCommandOutcome`,
`SpecifiedNextCliCommandOutcome`, `ProvidedNextCliCommandOutcome`,
`AggregatorOutcome`, `TableBuilderOutcome`, `AggregatorFilterOutcome`,
`ReactionOutcome`, `SpecifiedReactionOutcome`, `NothingOutcome`. Eight of
those carry state for a later command; `NothingOutcome` is the deliberate
silent ending. **None of them
is a missing writer.**

To change how a built-in outcome renders, register your own writer ahead of
`AddCli`, which runs `AddCommandAbstractions` internally. Anything added
afterwards lands behind the built-ins and never wins.

An `ExceptionOutcome` never prints: `CliApp` rethrows before
`WriteOutcomes` runs, under either host.

## Gaps

- An unmatched outcome is dropped in silence, and nothing distinguishes
  that from a forgotten writer.
  [#18](https://github.com/KitCli/KitCli/issues/18)
- Writers cast unchecked instead of pattern-matching, so calling `Write`
  without `CanWriteFor` throws `InvalidCastException`.
  [#117](https://github.com/KitCli/KitCli/issues/117)
- `ExceptionOutcomeIoWriter` lives in `CliCommandOutcomeIo.cs`, one of
  several filename mismatches.
  [#35](https://github.com/KitCli/KitCli/issues/35)

## See also

[0006-outcomes.md](0006-outcomes.md) · [0003-cli-io.md](0003-cli-io.md) ·
[0002-cli-app-host.md](0002-cli-app-host.md)
