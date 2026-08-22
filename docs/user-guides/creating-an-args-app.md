# Creating an args app

## What this is for

An args app runs one command per process invocation, driven straight
from `argv` — `dotnet run -- /greet --name Alex` — then exits. Use it
for scripting, CI, or anything meant to be called once and finish,
rather than a session someone sits in (for that, see
[creating-a-terminal-app.md](creating-a-terminal-app.md)).

## How to do it

Subclass `ArgsCliApp` (there's no ready-made basic version the way
there is for terminal apps — every args app needs at least a
constructor forwarding to the base class):

```csharp
public class MyArgsApp(ICliWorkflow workflow, ICliIo io) : ArgsCliApp(workflow, io);
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithApp<MyArgsApp>()
    .WithRegistry<MyAppRegistry>();

await app.Run(args);
```

`args` (everything after `--` on the command line) is joined into a
single ask string and parsed exactly like a line of terminal input —
`dotnet run -- /greet --name Alex` resolves to the same
`GreetCliCommand` a terminal app's `/greet --name Alex` would.

You get the same lifecycle hooks as a terminal app
(`OnSessionStart`, `OnRunComplete`, etc. — see
[creating-a-terminal-app.md](creating-a-terminal-app.md)) if you want
to observe or log the single run.

## Common mistakes

**Expecting a chained command to run to completion in one invocation.**
An args app runs `RespondToAsk` exactly once and stops — if the
resolved command queues up a next step with `ByMovingToCommand` (see
[chaining-commands.md](chaining-commands.md)), that next step is
never executed. If a one-shot invocation needs to do several things,
do them all in one handler instead of chaining.

**Calling `app.Run()` with no arguments.** `ArgsCliApp` requires at
least one argument — running it with none throws, by design, rather
than silently doing nothing.

**Reaching for an args app when you actually want scripted, repeated
input.** If you need to drive several asks programmatically (not from
a human typing), that's still better modeled as one args-app
invocation per ask, or a terminal app fed by piped input — not a
single args-app call expecting to process multiple asks.

## Learn more

- [creating-a-terminal-app.md](creating-a-terminal-app.md) — the
  interactive alternative, and the shared lifecycle hooks both modes
  offer.
- [chaining-commands.md](chaining-commands.md) — why chained commands
  don't fully play out in a one-shot invocation.
- [docs/concepts/cli-app-host.md](../concepts/cli-app-host.md) —
  exactly what `ArgsCliApp.Run` does, and why it's a compile-time
  choice between this and `TerminalCliApp` rather than a runtime flag.
