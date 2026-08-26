# 0003. Creating an interactive app

## What this is for

An interactive app asks the user for input, one line at a time, until they
exit — what most CLI tools call a REPL. Use it for a session someone sits
in. For a single invocation driven from `argv`, see
[0002-creating-a-headless-app.md](0002-creating-a-headless-app.md).

## How to do it

The plain version needs no code of your own:

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicApp()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

Run it and it prompts in a loop, executing whatever command each line
resolves to, until a command ends the session (see
[0012-exiting-the-app.md](0012-exiting-the-app.md)) or the input runs out.

### Hooking into the session lifecycle

To act at specific points — announce a session start, log every run,
customize output — subclass `CliApp`, override the hooks you need, and
register it with `WithApp<TCliApp>()` instead of `WithBasicApp()`:

```csharp
public class MyApp(ICliWorkflow workflow, ICliIo io) : CliApp(workflow, io)
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
    .WithApp<MyApp>()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

All six hooks — `OnSessionStart`, `OnRunCreated`, `OnRunStarted`,
`OnMovingPastAsk`, `OnRunComplete`, `OnSessionEnd` — are `virtual` and do
nothing by default. Override only what you need.

## Common mistakes

**Subclassing `CliApp` to change static output text.** To reword a
built-in message, write an `IOutcomeIoWriter` (see
[docs/concepts/0004-outcome-writing.md](../concepts/0004-outcome-writing.md)), not a
lifecycle hook. Hooks govern *when* something happens, writers *how* an
outcome renders.

**Doing expensive work in `OnRunStarted` or `OnMovingPastAsk`, expecting
it to block the run.** Both fire while the run executes, neither before
nor after. Use them for status and progress, never for setup a run
depends on.

**Passing a `HeadlessCliApp` subclass to `WithApp<T>()`, then calling
`Run()` with no arguments.** The base class you extend decides interactive
or headless, and nothing switches at runtime. `CliAppBuilder.Run` throws an
`ArgumentException` naming the app type.

**Overriding `Run` to observe a session.** `Run` is `virtual` because
`HeadlessCliApp` replaces the loop wholesale; overriding it to log
something means owning the ask loop and every step's outcome writing.
Reach for a hook first.

## Learn more

- [0002-creating-a-headless-app.md](0002-creating-a-headless-app.md) — the
  one-invocation alternative.
- [0004-creating-a-registry.md](0004-creating-a-registry.md) — wiring up the
  commands, and settings, an app runs.
- [docs/concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — what
  `CliApp.Run` does each iteration, and when each hook fires.
