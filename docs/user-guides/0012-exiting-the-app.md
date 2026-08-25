# 0012. Exiting the app

## What this is for

Every interactive KitCli app needs a way for the user to end the session.
That comes built in.

## How to do it

Nothing to write. Any app built with `WithBasicTerminalApp()` or
`WithApp<TCliApp>()` registers an exit command already: type `/exit`, or
the shorthand `/e`, and the session ends cleanly.

### Ending the run from your own command

When your own command must end the session as part of doing something
else, inject `ICliWorkflow`, call `Stop()`, and return a `Final`-kind
outcome last, as the built-in exit command does:

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

`cliWorkflow.Stop()` sets the workflow's status, so the host loop stops
asking for input; the `Final` outcome ends the current run. Both are
needed. Stopping the workflow leaves an in-progress run running, and
ending the run leaves the host loop free to create another.

## Common mistakes

**Assuming a `Final` outcome alone stops the app.** It ends the *current
run*. In an interactive terminal app the host loop then starts a new run
and asks again, unless you also call `ICliWorkflow.Stop()`.

**Reaching for `Environment.Exit`, or throwing, to end the session.** Both
skip `OnSessionEnd` and any cleanup a consuming app's hooks perform. End a
session through `ICliWorkflow.Stop()`.

## Learn more

- [docs/concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  the real `ExitCliCommandHandler` this example mirrors, and what a `Final`
  outcome does to a run's state.
- [docs/concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — the host
  loop reading `Workflow.Status` each iteration, and the `OnSessionEnd`
  hook firing once it stops.
