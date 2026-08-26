# 0001. Use MediatR for command dispatch

Status: Accepted (retroactive — reconstructed from code, not original notes)
Date: 2026-07-24

## Context

A run resolves a command at runtime, through a factory it found by
reflection. It then has an object whose concrete type it does not know, and
has to reach the one handler written for that type. Doing that by hand
means a type switch that every command author has to edit.

## Decision

Model each `CliCommand` as an `IRequest<Outcome[]>` and dispatch it through
MediatR's open-generic `IRequestHandler<>` resolution.

## Alternatives considered

- **Hand-written type switch or visitor** — puts dispatch in one file every
  command author must touch, and grows with the command count.
- **Direct DI resolution of `ICliCommandHandler<T>`** — what MediatR does
  internally, minus the pipeline-behaviour seam if logging or validation is
  ever wanted.

## Consequences

Adds MediatR and MediatR.Contracts to the dispatch path for what is
structurally a single-request use case — heavier than strictly necessary,
but it solves the real type-erasure problem rather than being ceremony.
