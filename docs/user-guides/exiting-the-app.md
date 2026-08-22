# Exiting the app

## What this is for

Every interactive KitCli app needs a way for the user to end the
session. This is built in — you don't need to write it yourself.

## How to do it

Nothing to write. Any app built with `WithBasicTerminalApp()` or
`WithApp<TCliApp>()` already registers an exit command — type
`/exit` (or the shorthand `/e`) and the session ends cleanly.

### Ending the run from your own command

If your own command needs to end the session as part of doing
something else — not just via the built-in `/exit` — inject
`ICliWorkflow` and call `Stop()`, then return a `Final`-kind outcome
last, the same way the built-in exit command does:

```csharp
public class LogOutAndExitCliCommandHandler(ICliWorkflow cliWorkflow)
    : CliCommandHandler<LogOutAndExitCliCommand>
{
    public override Task<Outcome[]> HandleCommand(LogOutAndExitCliCommand command, CancellationToken ct)
    {
        cliWorkflow.Stop();

        return FinishThisCommand()
            .ByFinallySaying("Logged out. Exiting.")
            .EndAsync();
    }
}
```

`cliWorkflow.Stop()` sets the workflow's own status so the host loop
stops asking for new input; the `Final` outcome ends the current run.
You need both — stopping the workflow alone doesn't end an
in-progress run, and ending the run alone doesn't stop the host loop
from creating another one.

## Common mistakes

**Assuming a `Final` outcome alone stops the app.** It only ends the
*current run* — for an interactive terminal app, the host loop
immediately starts a new run and asks again unless you also call
`ICliWorkflow.Stop()`.

**Reaching for `Environment.Exit` or throwing to end the session.**
That skips `OnSessionEnd` and any cleanup a consuming app's lifecycle
hooks do — always end a session through `ICliWorkflow.Stop()`.

## Learn more

- [docs/concepts/workflow-run-state-machine.md](../concepts/workflow-run-state-machine.md) —
  the real `ExitCliCommandHandler` this guide's example mirrors, and
  what a `Final` outcome does to a run's state underneath.
- [docs/concepts/cli-app-host.md](../concepts/cli-app-host.md) — the
  host loop that checks `Workflow.Status` each iteration, and the
  `OnSessionEnd` hook that fires once it stops.
