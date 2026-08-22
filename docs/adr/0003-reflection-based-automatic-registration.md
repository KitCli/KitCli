# 0003. Discover commands, factories, and handlers by reflection over manual DI wiring

Status: Accepted (retroactive — reconstructed from KitCli and SpendfulnessCli
history, not original notes)
Date: 2026-07-26

## Context

SpendfulnessCli's original CLI implementation (the codebase KitCli was
later extracted from) registered every command generator by hand: each
`IGenericCommandGenerator` was added to the service collection one at a
time, keyed by its instruction name string
(`services.AddKeyedSingleton<IGenericCommandGenerator>("spare-money", ...)`,
per its `docs/adr/0010-cli-workflow-concept.md`). Adding a command meant
touching that central registration list — easy to forget, and no
compiler error if you did.

As the framework grew (artefact factories converting outcomes into
queryable data, MediatR handlers dispatching commands, and eventually
command factories themselves), the same shape of problem kept recurring:
some registerable type, discoverable by a shared base type or interface,
that a consuming application shouldn't have to enumerate by hand.

## Decision

Wherever KitCli needs to register "every implementation of X in a given
assembly," it reflection-scans that assembly at startup and registers
what it finds, instead of asking the consumer to list them:

- `AddCommandsFromAssembly` finds every `CliCommand` subtype and matches
  it to a `CliCommandFactory<>` subtype (or falls back to
  `BasicCliCommandFactory<>`), then registers MediatR handlers from the
  same assembly via `RegisterServicesFromAssembly` (see
  [command-registration.md](../concepts/command-registration.md)).
- `AddArtefactFactoriesForAssembly` finds every `ArtefactFactory<>` subtype
  and registers it, building closed generic types via `MakeGenericType` +
  `Activator.CreateInstance` for generic ones (see
  [artefacts.md](../concepts/artefacts.md)).

## Alternatives considered

- **Manual registration, one line per implementation** — what
  SpendfulnessCli did originally. Explicit and traceable, but every new
  command/factory/handler is a second place (beyond the type itself) that
  has to be remembered and kept in sync, with no compiler check if it
  isn't.
- **Keyed DI registration driven by an explicit name argument at
  registration time** — closer to what command factories still do today
  (see [command-registration.md](../concepts/command-registration.md)),
  but requires the registration call site to know the key up front, which
  reflection-based discovery derives automatically from the type instead.

## Consequences

Adding a new command, artefact factory, or command handler requires zero
DI registration code — write the class, and it's found. The cost:
`Activator.CreateInstance` requires a parameterless constructor on
anything discovered this way, and a class that exists in an assembly the
caller never told KitCli to scan silently isn't registered — there's no
startup error naming what was missed, only an absence a consumer has to
notice at runtime (see the "Constraints & tradeoffs" sections of
[artefacts.md](../concepts/artefacts.md) and
[command-registration.md](../concepts/command-registration.md) for the
concrete failure shapes this produces in each subsystem).
