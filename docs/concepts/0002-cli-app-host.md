# 0002. CliApp host loop

`CliApp` is the shell around a workflow: it sources asks, drives runs, and
hands the outcomes to a writer. Two subclasses supply the loop —
`TerminalCliApp` for an interactive session, `ArgsCliApp` for a one-shot
`myapp /command --flag value`.

```csharp
OnSessionStart();
Io.Pause();
SetUpEventHandlers();

while (Workflow.Status != CliWorkflowStatus.Stopped)
{
    var run = Workflow.NextRun();
    OnRunCreated(run);
    var outcomes = await ExecuteRunOperation(run);
    WriteOutcomes(outcomes, outcomeIoWriters);
    OnRunComplete(run, outcomes);
    Io.Pause();
}

OnSessionEnd(Workflow.Runs);
```

`ExecuteRunOperation` calls `MoveToNext()` when the run has a recorded
`MovePastAsk` change, and otherwise `Io.AskAsync` then `RespondToAsk`.

## The mode is a compile-time choice

Which subclass you extend decides args-driven or terminal-driven for the
life of the class; `CliAppBuilder.Run` dispatches on the concrete type and
throws if an `ArgsCliApp` gets no args
([ADR 0005](../adr/0005-args-driven-cli-app.md)).

**An args app runs exactly one command** — it calls `Workflow.Stop()` after
one `RespondToAsk`, so a queued `ByMovingToCommand` step never runs.

Six `protected virtual` hooks — `OnSessionStart`, `OnRunCreated`,
`OnRunStarted`, `OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd` — let an
app observe without overriding `Run`. None redirects flow, and
`OnRunStarted`/`OnMovingPastAsk` fire *while* the run executes, so they
suit progress indicators rather than setup. One-shot invocations fire five
of them once each; `OnMovingPastAsk` never fires.

An `ExceptionOutcome` makes `TerminalCliApp` rethrow, ending the session.
`InvalidAsk` and `ReachedFinalOutcome` let the loop continue.

## Everything outside a run is a singleton

`CliAppBuilder.Run` builds the provider with `ValidateScopes` and
`ValidateOnBuild` on, then resolves the `CliApp` and its writers once from
the root. Those live for the whole app; only handlers get per-run
instances. **A singleton depending on a `Scoped` service fails at
startup**, naming both types, instead of silently capturing one.

## See also

[0010-workflow-run-state-machine.md](0010-workflow-run-state-machine.md) ·
[0003-cli-io.md](0003-cli-io.md) · [0004-outcome-writing.md](0004-outcome-writing.md)
