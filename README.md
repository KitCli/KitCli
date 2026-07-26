# KitCli

Create an extensible CLI in minutes. A .NET framework for building
terminal apps: DI-driven command dispatch (via MediatR), with
Commands/Outcomes/Artefacts/Workflow layers on top for state that
carries across a session — page size, filters, "next page" — without
each command hand-rolling it.

> **The API below reflects `main`, not what's on NuGet.** The last
> published version predates a breaking rewrite of the command
> factory/handler shape by over five months — see
> [#60](https://github.com/KitCli/KitCli/issues/60) before assuming
> `dotnet add package KitCli` gets you this today.

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
    .WithBasicCli()
    .WithRegistry<HelloRegistry>();

await app.Run();
```

Run it and type `/hello` (or the shorthand, `/h`) — a command's
invocation name is derived from its type name automatically, not
declared anywhere: `HelloCliCommand` → `hello` / `h`.

## Learn more

- [`CONTRIBUTING.md`](CONTRIBUTING.md) — conventions, branching, how to
  propose a change.
- [`docs/concepts/`](docs/concepts/) — how each subsystem actually
  works today: [instruction parsing](docs/concepts/instruction-parsing-pipeline.md),
  the [workflow state machine](docs/concepts/workflow-run-state-machine.md),
  [outcomes](docs/concepts/outcomes.md) and
  [artefacts](docs/concepts/artefacts.md),
  [aggregators](docs/concepts/aggregators.md) and
  [tables](docs/concepts/tables.md).
- [`docs/adr/`](docs/adr/) — architectural decisions and why.
- [`docs/reviews/`](docs/reviews/) — past architectural reviews.

## Packages

`KitCli` (the umbrella package) plus 8 supporting packages
(`KitCli.Abstractions`, `KitCli.Instructions[.Abstractions]`,
`KitCli.Commands[.Abstractions]`, `KitCli.Workflow[.Abstractions]`,
`KitCli.Workflow.Commands`) — see
[`CONTRIBUTING.md`](CONTRIBUTING.md#versioning--releases) for how
they're versioned and released.
