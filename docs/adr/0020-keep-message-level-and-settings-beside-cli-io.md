# 0020. Keep MessageLevel and KitCliSettings beside ICliIo

Status: Accepted
Date: 2026-10-12

## Context

`BySaying(message, level)` hides a message below `KitCliSettings.MinimumMessageLevel`,
but only for outcomes. Code that writes straight to the console, such as
progress lines from a pipeline inside a command, had no way to use the same
rule, so each app wrote its own copy
([#310](https://github.com/KitCli/KitCli/issues/310)). The rule belongs on
`ICliIo`, but `ICliIo` lives in `KitCli.Abstractions` and `MessageLevel` and
`KitCliSettings` lived in `KitCli.Commands.Abstractions`, which depends on it.

## Decision

`MessageLevel` and `KitCliSettings` move to `KitCli.Abstractions.Io`, the
namespace of `ICliIo`. `ICliIo` gains `Say(message, level)`, and `CliIo`
takes an optional `KitCliSettings` to apply the minimum.

## Alternatives considered

- **An extension in `KitCli.Commands.Abstractions`.** Nothing moves, but the
  caller passes the settings in every time.
- **`Say(message, level)` that never filters.** Needs the move anyway and
  leaves each app to write the filter again.

## Consequences

- Code that names `MessageLevel` or `KitCliSettings` adds
  `using KitCli.Abstractions.Io;`. This breaks code that references
  `KitCli.Commands.Abstractions` directly. Apps that reference only the `KitCli`
  package are unaffected.
- An app with its own `ICliIo` implementation adds the new method.
- The outcome writers still do their own check, so the rule exists twice
  until they call `Say(message, level)` instead.
