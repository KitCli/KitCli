# Specified vs provided outcomes

Handing off to a command or a reaction comes in two flavors, for both
`NextCliCommandOutcome` and reactions: build it yourself (**provided**),
or just name its type (**specified**) and let the run build it through
the normal factory path.

```csharp
.ByMovingToCommand(new ExportReport(reportId))  // provided — built here
.ByMovingToCommand<ExportReport>()              // specified — built by the run
.ByReacting(new SendEmail(address))             // provided
.ByReacting<SendEmail>()                        // specified
```

## What "the normal factory path" buys you

A provided command or reaction is already built, so nothing it needs can
come from the run — the handler must have everything in hand, by
constructor, right now.

A specified one is resolved later, the same way an instruction-resolved
command is: `CliWorkflowRun` puts the type through
`ICliWorkflowCommandProvider.GetCommand` (for the next command) or
`ICliWorkflowReactionProvider.GetReaction` (for a reaction), both of which
convert every prior outcome into artefacts first. **That's the whole
reason to specify instead of provide**: the factory sees the run's
accumulated artefacts, including outcomes the current command just
produced — data a handler, mid-execution, cannot build with yet.

`ByMovingToCommand<TCommand>(arguments)` adds one more thing a provided
command can't offer: factory arguments decided by the handler itself,
layered on top of whatever the run's artefacts already give the factory.

## Where this resolves

`CliWorkflowRun.GetNextCommandToMoveTo` and `.GetReactionToPublish` are
the two call sites. A provided value is unwrapped and returned as-is; a
specified one is turned into a fresh `Instruction` (for a command) or
resolved straight from `AllPriorOutcomes()` plus the current outcomes (for
a reaction) and handed to the matching provider.

## See also

[outcomes.md](outcomes.md) · [artefacts.md](artefacts.md) ·
[workflow-run-state-machine.md](workflow-run-state-machine.md)
