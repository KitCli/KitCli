# CliApp host loop

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

`ExecuteRunOperation` checks the run for a recorded `MovePastAsk` change.
Finding one it calls `MoveToNext()` and asks for no input; otherwise it
calls `Io.AskAsync` and passes the result to `RespondToAsk`.

## The mode is a compile-time choice

Which subclass you extend decides args-driven or terminal-driven for the
life of the class. `CliAppBuilder.Run` dispatches on the concrete type and
throws if an `ArgsCliApp` gets no args. See
[0005-args-driven-cli-app.md](../adr/0005-args-driven-cli-app.md).

**An args app runs exactly one command.** It calls `Workflow.Stop()`
unconditionally after one `RespondToAsk`, so a command that queues a next
step with `ByMovingToCommand` never gets to run it.

Six `protected virtual` hooks — `OnSessionStart`, `OnRunCreated`,
`OnRunStarted`, `OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd` — let an
app observe without overriding `Run`. None can redirect flow. In a one-shot
invocation five fire once each and `OnMovingPastAsk` never does.

`OnRunStarted` and `OnMovingPastAsk` fire while the run executes, not
before it, so they suit progress indicators and not setup.

An `ExceptionOutcome` makes `TerminalCliApp` rethrow the original
exception, ending the session. `InvalidAsk` and `ReachedFinalOutcome` both
let the loop continue.

## See also

[workflow-run-state-machine.md](workflow-run-state-machine.md) ·
[cli-io.md](cli-io.md) · [outcome-writing.md](outcome-writing.md)
