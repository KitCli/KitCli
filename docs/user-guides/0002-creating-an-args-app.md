# 0002. Creating an args app

## What this is for

An args app runs one command per process invocation, driven from `argv` —
`dotnet run -- /greet --name Alex` — then exits. Use it for scripting, CI,
or anything called once. For a session someone sits in, see
[0003-creating-a-terminal-app.md](0003-creating-a-terminal-app.md).

## How to do it

Subclass `ArgsCliApp`. No ready-made basic version exists, as terminal
apps have, because every args app needs a constructor forwarding to the
base class:

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

`args`, everything after `--` on the command line, joins into a single ask
string and parses exactly like a line of terminal input.
`dotnet run -- /greet --name Alex` resolves to the same `GreetCliCommand`
that a terminal app's `/greet --name Alex` would.

The lifecycle hooks a terminal app offers work here too — `OnSessionStart`,
`OnRunComplete`, and the rest (see
[0003-creating-a-terminal-app.md](0003-creating-a-terminal-app.md)) — for observing
or logging the single run. Five of the six fire, each once.
`OnMovingPastAsk` never fires, because an args app never continues a run
past its ask.

## Common mistakes

**Expecting a chained command to finish in one invocation.** An args app
calls `RespondToAsk` once and stops. Should the resolved command queue a
next step with `ByMovingToCommand` (see
[0007-chaining-commands.md](0007-chaining-commands.md)), that step never runs. When
a one-shot invocation must do several things, do them in one handler.

**Calling `app.Run()` with no arguments.** `ArgsCliApp` needs at least
one. Running it with none throws by design, rather than doing nothing.

**Reaching for an args app when you want scripted, repeated input.** To
drive several asks programmatically, use one args-app invocation per ask,
or a terminal app fed by piped input. A single args-app call processes one
ask.

## Learn more

- [0003-creating-a-terminal-app.md](0003-creating-a-terminal-app.md) — the
  interactive alternative, and the hooks both modes share.
- [0007-chaining-commands.md](0007-chaining-commands.md) — why chained commands stop
  short in a one-shot invocation.
- [docs/concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — what
  `ArgsCliApp.Run` does, and why the choice between it and
  `TerminalCliApp` is made at compile time.
