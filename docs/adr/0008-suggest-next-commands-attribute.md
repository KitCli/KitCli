# 0008. Suggest declared next commands via a `[CliNextCommandIs]` attribute

Status: Accepted
Date: 2026-08-23

## Context

A run parks at `ReachedReusableOutcome` once it reaches a reusable outcome
— paging through a list, say. If the next thing the user types resolves to
no command, the run made no state change and returned a `NothingOutcome`:
total silence, with no hint of what would have worked.

## Decision

Add `CliNextCommandIsAttribute`, an
`AttributeUsage(AttributeTargets.Class, AllowMultiple = true)` attribute
taking a `Name` and a `Description`, applied to the command type that
reaches the checkpoint:

```csharp
[CliNextCommandIs("next", "Show the next page.")]
[CliNextCommandIs("prev", "Show the previous page.")]
public record ShowPageCliCommand : CliCommand;
```

When an ask resolves to nothing and the run is already parked,
`CliWorkflowRun` reads the type of whichever command last ran — from the
`RanCliCommandOutcome` already in the run's history, so no new plumbing —
and returns one `SuggestionOutcome` per declared candidate.
`SuggestionOutcomeIoWriter` calls the existing `ICliIo.Pause()` before each
one, so every suggestion is visually separated. Each name is prefixed with
the configured `InstructionSettings.Prefix`, not a hardcoded `/`.

Declare none and the run still returns a silent `NothingOutcome`. Either
way the run's state is untouched: it stays parked exactly as before.

## Alternatives considered

- **A separate `PauseOutcome`/writer pair before every suggestion** — an
  earlier draft. Once the pause became unconditional it was emitted 1:1
  with a suggestion and had no other consumer, so folding it into
  `SuggestionOutcomeIoWriter` gives the same output with one fewer type.
- **Falling back to `InstructionConstants.DefaultNamePrefix` when the
  failed ask parsed no prefix** — a suggestion is synthesised by the run,
  not echoed back, so it should reflect the app's configured convention
  rather than a default that a reconfigured host would contradict.
- **A single free-text message, "Did you mean 'next'?"** — filler. The bare
  name plus its description reads clearly on its own.
- **Two `SayOutcome`s plus an empty one as a separator** — fakes a blank
  line through the message writer and gives the suggestion no outcome type
  to match on.
- **A `BySuggestingNextCommand` method on `OutcomeList`** — `OutcomeList`
  is the builder *handlers* use for their own outcomes. `CliWorkflowRun` is
  not a handler and builds raw `Outcome[]` everywhere else in that method.
- **One attribute taking an array** — settled the same way
  [ADR 0007](0007-cli-command-alias-attribute.md) settled it.

## Consequences

A command that parks a run can declare what to try next, with no new state
machinery, at the cost of one more attribute to know about. A command that
declares none behaves exactly as before, so this is purely additive.
