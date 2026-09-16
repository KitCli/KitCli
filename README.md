# KitCli

Create an extensible CLI in minutes. A .NET framework for building
terminal apps: DI-driven command dispatch (via MediatR), with
Commands/Outcomes/Artefacts/Workflow layers on top for state that
carries across a session — page size, filters, "next page" — without
each command hand-rolling it.

Read the documentation at
[kitcli.github.io/KitCli](https://kitcli.github.io/KitCli/).

- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Project structure](#project-structure)
- [Build and test](#build-and-test)
- [Kinds of documentation](#kinds-of-documentation)
- [Packages](#packages)
- [Contributing](#contributing)
- [License](#license)

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — every
  project targets `net10.0`.

## Installation

```bash
dotnet add package KitCli
```

`KitCli` is the umbrella package. It pulls in everything you need to build
an app; reference the others directly only when you are extending the
framework rather than consuming it.

## Quick start

```csharp
// A command — just a marker type.
public record HelloCliCommand : CliCommand;

// Decides when this command applies, and builds it.
public class HelloCliCommandFactory : CliCommandFactory<HelloCliCommand>
{
    public override bool CanCreateWhen() => true;
    public override CliCommand Create() => new HelloCliCommand();
}

// Does the actual work.
public class HelloCliCommandHandler : CliCommandHandler<HelloCliCommand>
{
    public override Task<Outcome[]> HandleCommand(HelloCliCommand command, CancellationToken ct)
        => FinishThisCommand()
            .ByFinallySaying("Hello, World!")
            .EndAsync();
}

// Registers every command in this assembly.
public class HelloRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
        => services.AddCommandsFromAssembly(typeof(HelloCliCommand).Assembly);
}
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicApp()
    .WithRegistry<HelloRegistry>();

await app.Run();
```

Run it and type `/hello` (or the shorthand, `/h`) — a command's
invocation name is derived from its type name automatically, not
declared anywhere: `HelloCliCommand` → `hello` / `h`.


## Project structure

```
KitCli/                              the umbrella package: CliApp, CliAppBuilder
KitCli.Abstractions/                 ICliIo, Aggregator, Table
KitCli.Instructions[.Abstractions]/  parsing an ask into a typed Instruction
KitCli.Commands[.Abstractions]/      CliCommand, factories, outcomes, artefacts
KitCli.Workflow[.Abstractions]/      the run state machine
KitCli.Workflow.Commands/            built-in commands (/exit)

KitCli.Playground.*/                 runnable sample apps and scenarios
*.Tests, *.IntegrationTests/         six test projects
```


## Build and test

```bash
dotnet restore KitCli.sln
dotnet build KitCli.sln
dotnet test KitCli.sln
```

CI runs those three steps on every PR and every push to `main`, across all
six test projects. To see the framework running, start a playground app:

```bash
dotnet run --project KitCli.Playground.App                # interactive
dotnet run --project KitCli.Playground.App.Headless -- /echo --name Alex   # headless
```

## Kinds of documentation

Everything below is published at
[kitcli.github.io/KitCli](https://kitcli.github.io/KitCli/), and each link
opens the page that starts that kind.

- [`CONTRIBUTING.md`](CONTRIBUTING.md) — conventions, branching, how to
  propose a change, and when to write each kind of doc below. Lives in the
  repository, not on the site.
- [User guides](https://kitcli.github.io/KitCli/docs/user-guides/0001-writing-a-basic-command.html)
  — how to do one task, without needing to know the machinery underneath.
- [Concepts](https://kitcli.github.io/KitCli/docs/concepts/0001-command-registration.html)
  — how each subsystem works today, for when a guide left you wondering why.
- [Decision records](https://kitcli.github.io/KitCli/docs/adr/0001-mediatr-for-command-dispatch.html)
  — a decision, its alternatives and what it cost, for when you want to
  change something and need the reason it is that way.
- [Investigations](https://kitcli.github.io/KitCli/docs/investigations/0002-which-extension-points-can-use-a-consumers-lifetimes.html)
  — what a spike found, verdict first, for when you are picking up the work
  it scoped.
- [Technology](https://kitcli.github.io/KitCli/docs/technology/microsoft-dependency-injection.html)
  — what KitCli's dependencies can do.
- [Reviews](https://kitcli.github.io/KitCli/docs/reviews/0001-architectural-review.html)
  — past architectural reviews. Historical only; never current state.

The [documentation home](https://kitcli.github.io/KitCli/docs/README.html)
is the full index, and the
[roadmap](https://kitcli.github.io/KitCli/docs/roadmap.html) tells how the
framework got here, month by month.

## Packages

`KitCli` (the umbrella package) plus 8 supporting packages
(`KitCli.Abstractions`, `KitCli.Instructions[.Abstractions]`,
`KitCli.Commands[.Abstractions]`, `KitCli.Workflow[.Abstractions]`,
`KitCli.Workflow.Commands`) — see
[`CONTRIBUTING.md`](CONTRIBUTING.md#versioning--releases) for how
they're versioned and released.

Packages do not ship in lockstep. Only what changed gets a new version, so
upgrading the umbrella still delivers lower-level fixes through its
dependencies. Whether that is the right model is owed an ADR —
[#128](https://github.com/KitCli/KitCli/issues/128).

## Contributing

Read [`CONTRIBUTING.md`](CONTRIBUTING.md) first — it covers Conventional
Commit titles, the label taxonomy, PR size limits, and when a change needs
an ADR or a concept doc.

## License

[MIT](LICENSE).
