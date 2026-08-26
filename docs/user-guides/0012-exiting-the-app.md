# 0012. Exiting the app

## What this is for

Every interactive KitCli app needs a way for the user to end the session.
That comes built in.

## How to do it

Nothing to write. Any app built with `WithBasicApp()` or
`WithApp<TCliApp>()` registers an exit command already: type `/exit`, or
the shorthand `/e`, and the session ends cleanly.

### Ending the session from your own command

When your own command must end the session as part of doing something else,
inject `ICliWorkflow`, call `Stop()`, and return a `Final`-kind outcome
last — as the built-in exit command does:

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

**Both halves are needed.** `Stop()` sets the workflow's status, so the
host loop stops asking for input; the `Final` outcome ends the current run.
Stopping the workflow alone leaves an in-progress run running, and ending
the run alone leaves the host loop free to create another.

## Common mistakes

**Assuming a `Final` outcome alone stops the app.** It ends the *current
run*. An interactive host then starts a new one and asks again, unless you
also call `ICliWorkflow.Stop()`.

**Reaching for `Environment.Exit`, or throwing, to end the session.** Both
skip `OnSessionEnd` and any cleanup a consuming app's hooks perform.

## Learn more

- [../concepts/0010-workflow-run-state-machine.md](../concepts/0010-workflow-run-state-machine.md) —
  what a `Final` outcome does to a run's state.
- [../concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — the host
  loop reading `Workflow.Status` each iteration, and `OnSessionEnd` firing
  once it stops.
