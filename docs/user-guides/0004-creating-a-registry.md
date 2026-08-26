# 0004. Creating a registry

## What this is for

Before `CliAppBuilder` builds the DI container, it needs to know which
assemblies hold your commands, and what services those commands depend on.
A registry tells it: one class, wired in with `WithRegistry<T>()`.

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
    .WithBasicApp()
    .WithRegistry<MyAppRegistry>();

await app.Run();
```

`AddCommandsFromAssembly` scans the assembly for every `CliCommand` and
wires up its factory and MediatR handler. Call it once per assembly that
defines commands. The assembly holding your `Program.cs` is scanned
already, so a registry serves commands living elsewhere.

Register any other services your handlers and factories need exactly as you
would in any DI setup.

### If your commands remember anything

A *second*, separate call registers artefact factories, which is what lets
a later command read what an earlier one produced:

```csharp
var assembly = typeof(GreetCliCommand).Assembly;

services.AddCommandsFromAssembly(assembly);
services.AddArtefactFactoriesForAssembly(assembly);
```

Add it whenever a command remembers state (see
[0009-remembering-your-own-state.md](0009-remembering-your-own-state.md)),
a factory calls `LastCommandWas<T>()`, or a table needs a "next page" step
(see [0011-showing-a-paged-table.md](0011-showing-a-paged-table.md)). It
registers the built-in artefact factories as well as yours, so all three
depend on it.

### When a registry needs settings

When registration depends on configuration — an API key, a connection
string — implement `IConfigurableCliAppRegistry<TSettings>` instead. Pair
it with `WithJsonSettings` or `WithUserSecretSettings`, plus the
two-type-parameter `WithRegistry<TSettings, TRegistry>()`:

```csharp
public class MyConfiguredRegistry : IConfigurableCliAppRegistry<MySettings>
{
    public void Register(MySettings settings, IServiceCollection services)
    {
        services.AddSingleton(new MyApiClient(settings.ApiKey));
        services.AddCommandsFromAssembly(typeof(GreetCliCommand).Assembly);
    }
}
```

Build it with `.WithJsonSettings("appsettings.json")` before
`.WithRegistry<MySettings, MyConfiguredRegistry>()`.

The configuration section name is `TSettings`'s type name with `Settings`
removed, so `MySettings` reads the `My` section. To read a different
section, rename the settings type; nothing names it explicitly.

## Common mistakes

**Calling `WithRegistry<TSettings, TRegistry>()` with no preceding
`With...Settings` call.** `CliAppBuilder` throws. It needs a configuration
source before it can bind settings to your type.

**Registering one assembly's commands from two registries.** Call
`AddCommandsFromAssembly` exactly once per assembly, and remember your
`Program.cs` assembly counts as registered already.

**Forgetting `AddArtefactFactoriesForAssembly`.** Startup stays silent. The
first symptom is a later command's `GetRequiredArtefact` throwing at
runtime, or `LastCommandWas<T>()` quietly returning `false`.

**Passing a marker type from the wrong assembly.** Point
`AddCommandsFromAssembly` at an assembly holding no commands and it throws
`ArgumentException` ("No ICommand Implementations Found"); point it at the
wrong one holding some, and your intended commands go unregistered in
silence while another assembly's register instead.

## Learn more

- [0001-writing-a-basic-command.md](0001-writing-a-basic-command.md) — what
  `AddCommandsFromAssembly` finds and registers.
- [../concepts/0001-command-registration.md](../concepts/0001-command-registration.md) —
  the full mechanics behind that call.
- [../concepts/0008-artefacts.md](../concepts/0008-artefacts.md) — what the
  second call registers.
