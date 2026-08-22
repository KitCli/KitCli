# Creating a terminal app

## What this is for

A terminal app is the interactive mode: it keeps asking the user for
input, one line at a time, until they exit — the mode most CLI tools
mean by "REPL." Use it when the app is a session someone sits in,
rather than a single one-shot invocation (for that, see
[creating-an-args-app.md](creating-an-args-app.md)).

## How to do it

The plain version needs no code of your own:

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicTerminalApp()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

Run it, and it prompts for input in a loop, executing whatever
command each line resolves to, until a command ends the session (see
[exiting-the-app.md](exiting-the-app.md)).

### Hooking into the session lifecycle

If you need to do something at specific points — announce a session
start, log every run, customize output — subclass `TerminalCliApp`
and override the lifecycle hooks you need, then register it with
`WithApp<TCliApp>()` instead of `WithBasicTerminalApp()`:

```csharp
public class MyTerminalApp(ICliWorkflow workflow, ICliIo io) : TerminalCliApp(workflow, io)
{
    protected override void OnSessionStart()
    {
        Io.Say("Welcome!");
        Io.SetTitle("My App");
    }

    protected override void OnRunComplete(ICliWorkflowRun run, Outcome[] outcomes)
    {
        Io.Say($"Run took {run.State.Stopwatch.ElapsedMilliseconds}ms.");
    }
}
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithApp<MyTerminalApp>()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

All six hooks (`OnSessionStart`, `OnRunCreated`, `OnRunStarted`,
`OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd`) are `virtual` and
default to doing nothing — override only the ones you need.

## Common mistakes

**Subclassing `TerminalCliApp` just to change static output text.**
If all you want is different wording for built-in messages, that's
likely an `IOutcomeIoWriter` concern, not a lifecycle hook — hooks are
for *when* something happens, not *how* a specific outcome renders.

**Doing expensive work in `OnRunStarted`/`OnMovingPastAsk` expecting it
to block the run.** These fire concurrently with the run actually
executing, not before or after it — they're for status/progress
reporting, not synchronous setup a run depends on.

**Registering commands before calling `WithApp`/`WithBasicTerminalApp`
and expecting order not to matter.** It doesn't, in practice — but
don't rely on registration order across `With...` calls for anything
beyond what's documented for command/factory resolution itself (see
[docs/concepts/command-registration.md](../concepts/command-registration.md)).

## Learn more

- [creating-an-args-app.md](creating-an-args-app.md) — the one-shot
  alternative, for a single invocation instead of a session.
- [creating-a-registry.md](creating-a-registry.md) — wiring up the
  commands (and settings, if needed) a terminal app runs.
- [docs/concepts/cli-app-host.md](../concepts/cli-app-host.md) — what
  `TerminalCliApp.Run` actually does each iteration, and exactly when
  each lifecycle hook fires relative to it.
