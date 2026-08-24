# Outcome writing

An `IOutcomeIoWriter` turns one outcome into output. `CliApp.WriteOutcomes`
matches each outcome against the writer list and takes the **first** whose
`CanWriteFor` returns `true` — the same first-match-wins rule used for
command factories and argument builders.

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
order. Because no two claim the same type, first-match-wins decides nothing
today; it is the rule waiting for the first writer that claims a base type.

## Silence is often deliberate

Seven of the fifteen outcome types have no writer at all —
`RanCliCommandOutcome`, `NextCliCommandOutcome`, `AggregatorOutcome`,
`TableBuilderOutcome`, `AggregatorFilterOutcome`, `ReactionOutcome`,
`NothingOutcome`. Six carry state for a later command; the last is the
deliberate silent ending. **None of them is a missing writer.**

To change how a built-in outcome renders, register your own writer ahead of
`AddCli`, which runs `AddCommandAbstractions` internally. Anything added
afterwards lands behind the built-ins and never wins.

An `ExceptionOutcome` prints in an args app but not an interactive one:
`TerminalCliApp` rethrows before `WriteOutcomes` runs.

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

[outcomes.md](outcomes.md) · [cli-io.md](cli-io.md) ·
[cli-app-host.md](cli-app-host.md)
