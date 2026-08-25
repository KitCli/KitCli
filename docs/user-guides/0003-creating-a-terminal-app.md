# 0003. Creating a terminal app

## What this is for

A terminal app is the interactive mode: it asks the user for input, one
line at a time, until they exit — what most CLI tools call a REPL. Use it
for a session someone sits in. For a single one-shot invocation, see
[0002-creating-an-args-app.md](0002-creating-an-args-app.md).

## How to do it

The plain version needs no code of your own:

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicTerminalApp()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

Run it and it prompts in a loop, executing whatever command each line
resolves to, until a command ends the session (see
[0012-exiting-the-app.md](0012-exiting-the-app.md)).

### Hooking into the session lifecycle

To act at specific points — announce a session start, log every run,
customize output — subclass `TerminalCliApp`, override the hooks you need,
and register it with `WithApp<TCliApp>()` instead of
`WithBasicTerminalApp()`:

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

All six hooks — `OnSessionStart`, `OnRunCreated`, `OnRunStarted`,
`OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd` — are `virtual` and do
nothing by default. Override only what you need.

## Common mistakes

**Subclassing `TerminalCliApp` to change static output text.** To reword a
built-in message, write an `IOutcomeIoWriter` (see
[docs/concepts/0004-outcome-writing.md](../concepts/0004-outcome-writing.md)), not a
lifecycle hook. Hooks govern *when* something happens, writers *how* an
outcome renders.

**Doing expensive work in `OnRunStarted` or `OnMovingPastAsk`, expecting
it to block the run.** Both fire while the run executes, neither before
nor after. Use them for status and progress, never for setup a run
depends on.

**Passing an `ArgsCliApp` subclass to `WithApp<T>()`, then calling `Run()`
with no arguments.** The base class you extend decides interactive or
one-shot, and nothing switches at runtime. `CliAppBuilder.Run` throws an
`ArgumentException` naming the app type.

**Overriding `Run` instead of using the hooks.** It isn't sealed, so you
can, but you then own the ask-versus-continue routing and outcome writing
that `TerminalCliApp.Run` already handles. Reach for a hook first.

## Learn more

- [0002-creating-an-args-app.md](0002-creating-an-args-app.md) — the one-shot
  alternative.
- [0004-creating-a-registry.md](0004-creating-a-registry.md) — wiring up the
  commands, and settings, a terminal app runs.
- [docs/concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — what
  `TerminalCliApp.Run` does each iteration, and when each hook fires.
