# Creating a registry

## What this is for

`CliAppBuilder` needs to know which assemblies contain your commands
(and any other services they depend on) before it builds the app's DI
container. A registry is where you tell it — one class, wired in with
`WithRegistry<T>()`.

## How to do it

For the common case — your commands don't need any configuration to
register — implement `ICliAppRegistry`:

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

`AddCommandsFromAssembly` scans the given assembly for every
`CliCommand` and wires up its factory and MediatR handler
registration — call it once per assembly that defines commands.
Register any other services your handlers/factories need the same way
you would in any DI setup (`services.AddSingleton<...>()`, etc.).

### When a registry needs settings

If registering your commands depends on configuration — an API key, a
connection string — implement `IConfigurableCliAppRegistry<TSettings>`
instead, and pair it with `WithJsonSettings`/`WithUserSecretSettings`
plus the two-type-parameter `WithRegistry<TSettings, TRegistry>()`:

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

`WithSettings<T>` derives the configuration section name by stripping
`Settings` off `TSettings`'s type name — `MySettings` reads the `My`
section.

## Common mistakes

**Calling `WithRegistry<TSettings, TRegistry>()` without a preceding
`With...Settings` call.** `CliAppBuilder` throws — it needs a
configuration source set up before it can bind settings to your type.

**Registering the same assembly's commands from more than one
registry.** `AddCommandsFromAssembly` isn't designed to be called
twice for the same assembly — call it exactly once per assembly, from
one registry.

**Passing a marker type from the wrong assembly to
`AddCommandsFromAssembly`.** It scans whichever assembly
`typeof(SomeType).Assembly` resolves to — if that's not the assembly
your commands actually live in, you either get an `ArgumentException`
("No ICommand Implementations Found") if that assembly has none at
all, or your intended commands silently never get registered while a
different assembly's do.

## Learn more

- [writing-a-basic-command.md](writing-a-basic-command.md) — what
  `AddCommandsFromAssembly` actually finds and registers.
- [docs/concepts/command-registration.md](../concepts/command-registration.md) —
  the full mechanics of what happens when a registry calls
  `AddCommandsFromAssembly`.
