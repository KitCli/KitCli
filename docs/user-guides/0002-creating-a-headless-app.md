# 0002. Creating a headless app

## What this is for

A headless app has nothing attached to its input. It takes one ask from the
command line — `dotnet run -- /greet --name Alex` — runs it, and exits. Use
it for scripting, CI, or anything invoked once. For a session someone sits
in, see
[0003-creating-an-interactive-app.md](0003-creating-an-interactive-app.md).

## How to do it

Subclass `HeadlessCliApp`. There is no ready-made basic version, as
interactive apps have, because every headless app needs a constructor
forwarding to the base class:

```csharp
public class MyHeadlessApp(ICliWorkflow workflow, ICliIo io) : HeadlessCliApp(workflow, io);
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithApp<MyHeadlessApp>()
    .WithRegistry<MyAppRegistry>();

await app.Run(args);
```

`args` — everything after `--` on the command line — joins into a single
ask string and parses exactly like a line of typed input.
`dotnet run -- /greet --name Alex` resolves to the same `GreetCliCommand`
that an interactive app's `/greet --name Alex` would.

A command that chains to another with `ByMovingToCommand` (see
[0007-chaining-commands.md](0007-chaining-commands.md)) runs every step
here, same as anywhere else. The limit is not how many commands run — it is
that they all belong to one run, and no second run can start.

All six lifecycle hooks work (see
[0003-creating-an-interactive-app.md](0003-creating-an-interactive-app.md)),
`OnMovingPastAsk` firing once per chained step.

## Common mistakes

**Calling `app.Run()` with no arguments.** A headless app has no way to ask
for one, so `CliAppBuilder.Run` throws an `ArgumentException` naming your
app type rather than sitting there doing nothing.

**Expecting a command to prompt for something.** Where an interactive
session would stop at a reusable checkpoint and take another ask, a
headless session ends there with the run unfinished. Everything a command
needs has to arrive in the args.

**Reaching for a headless app when you want scripted, repeated input.** To
drive several asks programmatically, use one invocation per ask, or an
interactive app fed by piped input.

## Learn more

- [0003-creating-an-interactive-app.md](0003-creating-an-interactive-app.md) — the
  interactive alternative, and the hooks both modes share.
- [0007-chaining-commands.md](0007-chaining-commands.md) — running several commands
  from one ask.
- [../concepts/0002-cli-app-host.md](../concepts/0002-cli-app-host.md) — what
  `HeadlessCliApp.Run` does, and why the choice between the two hosts is
  made at compile time.
