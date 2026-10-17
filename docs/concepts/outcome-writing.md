# Outcome writing

**A handler returns outcomes and never prints. An `IOutcomeIoWriter` turns each outcome into output, so you restyle a message without touching the command that produced it.**

| Term | Meaning |
|---|---|
| Outcome | What a command returns. |
| `IOutcomeIoWriter` | Claims one outcome type and writes it through `ICliIo`. |
| `MessageLevel` | How serious a message is, `Trace` through `Error`. |

```mermaid
flowchart LR
    H["handler"] -->|returns| O["Outcome[]"]
    O --> W["CliApp.WriteOutcomes"]
    W -->|"first CanWriteFor true"| WR["IOutcomeIoWriter"]
    WR -->|Write| IO["ICliIo"]
```

```csharp
public interface IOutcomeIoWriter
{
    bool CanWriteFor(Outcome outcome);
    void Write(Outcome outcome);
}
```

`CliApp.WriteOutcomes` matches each outcome against the writer list and takes the **first** whose `CanWriteFor` returns `true`. Command factories and argument builders follow the same rule.

## The nine built-in writers

`AddCommandAbstractions` registers nine writers, each claiming exactly one outcome type: NotFound, Output, Message, Table, PageSize, PageNumber, Suggestion, Pause and Exception.

| Fact | Consequence |
|---|---|
| `CliAppBuilder.Run` resolves writers once from the root provider. | DI registration order becomes match order. |
| The root provider has no scope. | A writer taking a `Scoped` dependency fails at startup. Give it `ICliIo`, or read per-run data off the `Outcome`. |
| No two built-ins claim the same type. | First-match-wins decides nothing today. It waits for the first writer that claims a base type. |

**To change how a built-in outcome renders, register your own writer ahead of `AddCli`.** `AddCli` runs `AddCommandAbstractions` internally, so anything added afterwards lands behind the built-ins and never wins.

## Silence is often deliberate

Half the outcome types have no writer. **None of them is a missing writer.**

| Outcomes without a writer | Why |
|---|---|
| `RanCliCommandOutcome`, `SpecifiedNextCliCommandOutcome`, `ProvidedNextCliCommandOutcome`, `AggregatorOutcome`, `TableBuilderOutcome`, `AggregatorFilterOutcome`, `ReactionOutcome`, `SpecifiedReactionOutcome` | Carry state for a later command. |
| `NothingOutcome` | The deliberate silent ending. |

An `ExceptionOutcome` never prints: `CliApp` rethrows before `WriteOutcomes` runs, under either host.

## Leveled messages

`BySaying(message, level)` and `ByFinallySaying(message, level)` append a `LeveledSayOutcome` or `LeveledFinalSayOutcome`. These subclass `SayOutcome` and `FinalSayOutcome` and carry a `MessageLevel`. `MessageOutcomeIoWriter` and `OutputOutcomeIoWriter` skip the write when the level falls short of `KitCliSettings.MinimumMessageLevel`. A plain `SayOutcome` or `FinalSayOutcome` has no level and always writes.

`MinimumMessageLevel` defaults to `Information`. It binds the same way as `InstructionSettings`: from a `KitCli` section if `WithJsonSettings` or `WithUserSecretSettings` loaded one, the default otherwise. No `WithSettings<KitCliSettings>()` call is needed either way.

## Gaps

- An unmatched outcome is dropped in silence, and nothing distinguishes that from a forgotten writer: [#18](https://github.com/KitCli/KitCli/issues/18).
- Writers cast unchecked instead of pattern-matching, so calling `Write` without `CanWriteFor` throws `InvalidCastException`: [#117](https://github.com/KitCli/KitCli/issues/117).
- Some files do not match the type they hold: [#35](https://github.com/KitCli/KitCli/issues/35).

## See also

- [Outcomes](outcomes.md): the kinds and lifetimes being written.
- [CLI I/O](cli-io.md): the `ICliIo` writers call.
- [CliApp host loop](cli-app-host.md): where `WriteOutcomes` runs.
- [App configuration](app-configuration.md): how settings bind.
