# 0001. Use MediatR for command dispatch

Status: Accepted (retroactive — reconstructed from code, not original notes)
Date: 2026-07-24

## Context

A `CliWorkflowRun` resolves a concrete `CliCommand` instance dynamically via
a reflection-driven `ICliCommandFactory`, then must route that runtime-typed
object to the one `CliCommandHandler<TCliCommand>` that matches it, without
a hand-written type switch growing per command.

## Decision

Model each `CliCommand` as an `IRequest<Outcome[]>` and dispatch it through
MediatR's open-generic `IRequestHandler<>` resolution.

## Alternatives considered

- **Hand-written type switch / visitor** — doesn't scale as commands grow,
  and puts dispatch logic in one file every command author has to touch.
- **Direct DI resolution of `ICliCommandHandler<T>`** — equivalent to what
  MediatR does internally, but without pipeline behavior support if that's
  ever needed (logging, validation) later.

## Consequences

Adds a real dependency (MediatR + MediatR.Contracts) to the dispatch path
for what is, structurally, a single-request/no-notifications use case —
heavier than strictly necessary, but it solves the actual type-erasure
problem rather than being pure ceremony.
