# App configuration

`CliAppBuilder` loads configuration and resolves settings before `Run`
ever builds the container. Call `WithJsonSettings`/`WithUserSecretSettings`
to add a source, then `WithSettings<TSettings>` to bind a section of it.

```csharp
new CliAppBuilder()
    .WithBasicApp()
    .WithJsonSettings("appsettings.json")
    .WithSettings<MySettings>()
    .WithRegistry<MyRegistry>()
    .Run(args);
```

## A section is the type name, minus "Settings"

`WithSettings<MySettings>` looks up a section called `My` — the type name
with any trailing `Settings` stripped. There's no other way to name it.
Calling it before any `With[..]Settings` call throws: nothing has built a
configuration to read from yet.

## Two types never need `WithSettings`

`InstructionSettings` and `KitCliSettings` resolve through the same
lookup, but fall back to their own defaults instead of throwing — whether
no configuration source was ever added, or one was but the section isn't
in it. Every other settings type requires both: a source, and a section
that exists in it. `Run` calls this lookup for both built-in types itself,
so an app that never calls `WithSettings<KitCliSettings>()` still gets one.

## A registry runs at build time, not at resolution time

`WithRegistry<TRegistry>()` constructs `TRegistry` and calls
`Register(services)` immediately — before the container is built, not
when something resolves it later. `WithRegistry<TSettings, TRegistry>()`
resolves `TSettings` through the section lookup above first, then passes
it to `Register(settings, services)`. Either way, registration happens
once, synchronously, while the builder chain runs.

## See also

[cli-app-host.md](cli-app-host.md) · [command-registration.md](command-registration.md) ·
[outcome-writing.md](outcome-writing.md)
