# 0008. Suggest declared next commands via a `[CliNextCommandIs]` attribute

Status: Accepted
Date: 2026-08-23

## Context

`CliWorkflowRun.RespondToAsk` (`KitCli.Workflow/Run/CliWorkflowRun.cs`) parks
a run at `ReachedReusableOutcome` once it reaches a reusable outcome — e.g.
paging through a list. If the next typed input then doesn't resolve to any
command, `GetCommand` throws `NoCommandGeneratorException`, and the run
today makes no state change and returns `[new NothingOutcome()]`: total
silence, no hint about what would actually work next.

## Decision

Add `CliNextCommandIsAttribute` (`KitCli.Commands.Abstractions/CliNextCommandIsAttribute.cs`),
an `AttributeUsage(AttributeTargets.Class, AllowMultiple = true)` attribute
taking a `Name` and a `Description`, applied to the command type that
reaches the reusable checkpoint:

```csharp
[CliNextCommandIs("next", "Show the next page.")]
[CliNextCommandIs("prev", "Show the previous page.")]
public class ShowPageCliCommand : CliCommand { ... }
```

`TypeExtensions.GetCliNextCommandNames()` reads these off a type, alongside
the existing `GetCliCommandAliasNames()`.

In the `NoCommandGeneratorException` catch in `RespondToAsk`, when the run
is already parked at `ReachedReusableOutcome`: look up the type of whichever
command most recently ran (from the `RanCliCommandOutcome` already recorded
in the run's outcome history — no new plumbing needed), read its declared
next commands, and return one new `SuggestionOutcome` (`Name`, `Description`)
per candidate. It gets its own `IOutcomeIoWriter`
(`SuggestionOutcomeIoWriter`, alongside the existing `MessageOutcomeIoWriter`
etc.) that calls the already-existing `ICliIo.Pause()` before writing the
name and description, so every suggestion — including the first — is
visually separated from whatever preceded it, without a separate outcome
type or a second writer call per candidate. Each suggested name is prefixed
with the configured `InstructionSettings.Prefix`
(injected into `CliWorkflowRun` as `IOptions<InstructionSettings>`, the same
settings `InstructionTokenIndexer` parses instructions against), not a
hardcoded `/` — so a reconfigured prefix character is reflected in
suggestions too. On the console this renders as:

```

/next
Show the next page.

/prev
Show the previous page.
```

If no next commands are declared, the run still returns
`[new NothingOutcome()]` — this only improves the feedback given when a
command opts in; it does not invent wording for commands that haven't. No
state change either way: the run stays parked at `ReachedReusableOutcome`
exactly as it does today.

## Alternatives considered

- **A separate `PauseOutcome`/`PauseOutcomeIoWriter` pair, returned before
  every `SuggestionOutcome`** — an earlier draft did this (itself replacing
  an even earlier draft that only paused *between* candidates, via a
  `foreach` loop that skipped the pause for the first one). Once the pause
  became unconditional — every candidate, including the first, needs the
  visual separation, since `CliApp.WriteOutcomes` inserts none of its own —
  `PauseOutcome` was always emitted 1:1 with a `SuggestionOutcome` and had
  no other consumer anywhere in the codebase. Keeping it as its own
  outcome/writer pair was YAGNI: folding the pause into
  `SuggestionOutcomeIoWriter` itself (call `ICliIo.Pause()` before writing
  the name and description) gives the same rendered output with one fewer
  type and lets `SuggestNextCommands` collapse to a single `Select`, no
  interleaving logic at all.
- **Falling back to `InstructionConstants.DefaultNamePrefix` when the failed
  ask's own parsed `Instruction.Prefix` is null, instead of always reading
  the configured `InstructionSettings.Prefix`** — an earlier draft did
  this, but `DefaultNamePrefix` is only the *default value* of a
  configurable setting; if a host reconfigures its prefix character,
  suggestions built from the constant would silently use the wrong one.
  Since a suggestion is synthesized by the run, not an echo of what the
  user typed, it should always reflect the app's actual configured
  convention rather than either the value that happened to be parsed or a
  hardcoded default.
- **A single free-text message, e.g. `"Did you mean 'next'?"`** — rejected
  as unnecessary filler wording; the bare instruction name plus its own
  description reads clearly without a wrapping sentence, matching this
  repo's plain-language preference for CLI-facing messages.
- **Two raw `SayOutcome`s per candidate plus an empty-string `SayOutcome`
  as a separator** — an earlier draft of this change did exactly that, but
  it fakes a blank line through the message writer instead of using the
  `ICliIo.Pause()` method that already exists for it, and gives the
  suggestion no outcome type of its own to match on. Replaced with the
  dedicated `SuggestionOutcome` described above.
- **Building the suggestion via a new `OutcomeList.BySuggestingNextCommand`
  method** — rejected because `OutcomeList` is the fluent builder
  `CliCommandHandler<T>` implementations use for their own return outcomes;
  `CliWorkflowRun` isn't a handler and builds raw `Outcome[]` directly
  everywhere else in this method, so adding to `OutcomeList` would sit
  unused by this change.
- **A single `Description` array on one attribute instance** (mirroring the
  array-vs-repeatable question ADR 0007 already settled for
  `CliCommandAliasAttribute`) — rejected for the same reason: a repeatable
  single-pair attribute reads better at the declaration site than
  array-syntax arguments.

## Consequences

Command authors can now give a reusable-outcome command a declared set of
"what to try next" suggestions without any new state machinery, at the cost
of one more attribute to know about when reading what a command supports.
A command that reaches a reusable outcome but declares no
`[CliNextCommandIs]` behaves exactly as before — silent `NothingOutcome` —
so this is purely additive and doesn't change behavior for existing
commands.
