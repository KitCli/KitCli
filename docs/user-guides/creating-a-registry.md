# Creating a registry

## What this is for

Before `CliAppBuilder` builds the DI container, it needs to know which
assemblies hold your commands and the services they depend on. A registry
tells it: one class, wired in with `WithRegistry<T>()`.

## How to do it

When your commands need no configuration to register, implement
`ICliAppRegistry`:

```csharp
public class MyAppRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
        => services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
}
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicTerminalApp()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

`AddCommandsFromAssembly` scans the assembly for every `CliCommand` and
wires up its factory and MediatR handler. Call it once per assembly that
defines commands. The assembly holding your `Program.cs` is scanned
already, so a registry serves commands living elsewhere.

Register any other services your handlers and factories need as you would
in any DI setup: `services.AddSingleton<...>()`, and so on.

### If your commands use artefacts

A *second*, separate call registers artefact factories;
`AddCommandsFromAssembly` leaves them alone. Add it whenever a command in
the assembly remembers state for a later command to read back (see
[remembering-your-own-state.md](remembering-your-own-state.md)):

```csharp
public class MyAppRegistry : ICliAppRegistry
{
    public void Register(IServiceCollection services)
    {
        var assembly = typeof(GreetCliCommand).Assembly;

        services.AddCommandsFromAssembly(assembly);
        services.AddArtefactFactoriesForAssembly(assembly);
    }
}
```

That call also picks up artefact factories for every `Aggregator` and
`TableBuilder` subclass in the assembly, which is what makes a paged
table's "next page" work (see
[showing-a-paged-table.md](showing-a-paged-table.md)).

### When a registry needs settings

When registration depends on configuration — an API key, a connection
string — implement `IConfigurableCliAppRegistry<TSettings>` instead. Pair
it with `WithJsonSettings` or `WithUserSecretSettings`, plus the
two-type-parameter `WithRegistry<TSettings, TRegistry>()`:

```csharp
public class MySettings
{
    public string ApiKey { get; init; } = string.Empty;
}

public class MyConfiguredRegistry : IConfigurableCliAppRegistry<MySettings>
{
    public void Register(MySettings settings, IServiceCollection services)
    {
        services.AddSingleton(new MyApiClient(settings.ApiKey));
        services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
    }
}
```

```csharp
// Program.cs
var app = new CliAppBuilder()
    .WithBasicTerminalApp()
    .WithJsonSettings("appsettings.json")
    .WithRegistry<MySettings, MyConfiguredRegistry>();

await app.Run();
```

The configuration section name comes from `TSettings`'s type name with
`Settings` stripped off, so `MySettings` reads the `My` section. To use a
different section, rename the settings type; nothing names it explicitly.

## Common mistakes

**Calling `WithRegistry<TSettings, TRegistry>()` with no preceding
`With...Settings` call.** `CliAppBuilder` throws. It needs a configuration
source before it can bind settings to your type.

**Registering one assembly's commands from two registries.** Call
`AddCommandsFromAssembly` exactly once per assembly, from one registry,
and remember your `Program.cs` assembly counts as registered already.

**Forgetting `AddArtefactFactoriesForAssembly` when commands remember
state.** Startup stays silent. The first symptom is a later command's
`GetRequiredArtefact` throwing at runtime, reporting the artefact missing.

**Passing a marker type from the wrong assembly.**
`AddCommandsFromAssembly` scans whichever assembly
`typeof(SomeType).Assembly` resolves to. Point it at an assembly holding
no commands and it throws `ArgumentException` ("No ICommand
Implementations Found"); point it at the wrong assembly holding some, and
your intended commands silently go unregistered while another assembly's
register instead.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — what
  `AddCommandsFromAssembly` finds and registers.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  the full mechanics behind that call.
