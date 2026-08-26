# 0003. Discover commands, factories, and handlers by reflection over manual DI wiring

Status: Accepted (retroactive — reconstructed from KitCli and SpendfulnessCli
history, not original notes)
Date: 2026-07-26

## Context

The codebase KitCli was extracted from registered every command generator
by hand, one `AddKeyedSingleton` per instruction name. Adding a command
meant editing a central list — easy to forget, and no compiler error when
you did.

The same shape kept recurring as the framework grew: some registerable type
identified by a shared base type, that a consuming application should not
have to enumerate.

## Decision

Wherever KitCli needs "every implementation of X in a given assembly", it
reflection-scans that assembly at startup and registers what it finds:

- `AddCommandsFromAssembly` finds every `CliCommand` subtype, matches it to
  a `CliCommandFactory<>` (or falls back to `BasicCliCommandFactory<>`),
  and registers MediatR handlers from the same assembly. See
  [0001-command-registration.md](../concepts/0001-command-registration.md).
- `AddArtefactFactoriesForAssembly` finds every `ArtefactFactory<>`,
  building closed generic types with `MakeGenericType` and
  `Activator.CreateInstance`. See
  [0008-artefacts.md](../concepts/0008-artefacts.md).

## Alternatives considered

- **Manual registration, one line per implementation** — explicit and
  traceable, but every new type becomes a second place to remember, with no
  compiler check when it is missed.
- **Keyed DI driven by a name argument at registration time** — needs the
  call site to know the key up front, which reflection derives from the
  type instead.

## Consequences

Adding a command, artefact factory, or handler needs no DI code at all —
write the class and it is found. The costs:

- `Activator.CreateInstance` requires a parameterless constructor on
  anything discovered this way, so consumer artefact factories can take no
  dependencies.
- A class in an assembly nobody told KitCli to scan is silently
  unregistered. Nothing names what was missed; a consumer notices at
  runtime. See the "Gaps" sections of
  [0008-artefacts.md](../concepts/0008-artefacts.md) and
  [0001-command-registration.md](../concepts/0001-command-registration.md)
  for the failure shape in each subsystem.
